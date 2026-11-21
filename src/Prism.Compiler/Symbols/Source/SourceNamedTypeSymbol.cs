// @file SourceNamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Compiler.Binding;
using Prism.Compiler.Declarations;
using Prism.Compiler.Symbols.Synthesized;
using Prism.Constants;
using Prism.Diagnostics;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;
using Prism.Utils;
using ZLinq;

namespace Prism.Compiler.Symbols.Source;

internal sealed class SourceNamedTypeSymbol
    : SourceMemberContainerSymbol,
        INamedTypeSymbolData,
        IAttributeHolderSymbolData,
        IVisibilityGuardedSymbolData
{
    private readonly MergedTypeDeclaration _mergedDeclaration;
    private ImmutableArray<AttributeData> _attributes;
    private readonly Lock _memberChecksLock = new();
    private ThreeState _isDefaultConstructible;

    internal SourceNamedTypeSymbol(
        Compilation compilation,
        MergedTypeDeclaration mergedDeclaration,
        MemberContainerSymbol containingSymbol
    )
        : base(compilation)
    {
        _mergedDeclaration = mergedDeclaration;
        ContainingSymbol = containingSymbol;
        SpecialType = SpecialTypeFacts.FromMetadataName(
            mergedDeclaration.Name,
            containingSymbol as NamespaceSymbol
        );

        PublicSymbol = new NamedTypeSymbol(this);
        foreach (var decl in _mergedDeclaration.Declarations)
        {
            DeclaringCompilation.CacheSymbol(decl.SyntaxReference.Syntax, PublicSymbol);
        }
    }

    public new NamedTypeSymbol PublicSymbol
    {
        get => Unsafe.As<NamedTypeSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = Unsafe.As<NamedTypeSymbol>(value);
    }

    public override string Name => _mergedDeclaration.Name;

    public override Symbol ContainingSymbol { get; }

    public NamedTypeKind Kind => GetTypeKind(_mergedDeclaration);
    public SpecialType SpecialType { get; }

    private static NamedTypeKind GetTypeKind(MergedTypeDeclaration declaration)
    {
        return declaration.Kind switch
        {
            DeclarationKind.Namespace => throw new InvalidOperationException(
                "Type kind cannot be a namespace"
            ),
            DeclarationKind.Attribute => NamedTypeKind.Attribute,
            DeclarationKind.Class => NamedTypeKind.Class,
            DeclarationKind.Enum => NamedTypeKind.Enum,
            _ => throw new InvalidOperationException("Invalid declaration kind"),
        };
    }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [.. _mergedDeclaration.Declarations.Select(d => d.Location)],
                default
            );
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [.. _mergedDeclaration.Declarations.Select(d => d.SyntaxReference)],
                default
            );
            return field;
        }
    }
    public DeclaredVisibility DeclaredVisibility =>
        DeclaredVisibility.FromDeclarationModifiers(_mergedDeclaration.Modifiers);

    public ImmutableArray<EnumOrdinalSymbol> EnumOrdinals
    {
        get
        {
            return LazySymbols.LazyCompute(
                ref field,
                this,
                static (@this, context) => @this.ComputeEnumOrdinals(context),
                static (@this, context) => @this.AddDeclarationDiagnostics(context)
            );
        }
    }

    private ImmutableArray<EnumOrdinalSymbol> ComputeEnumOrdinals(BindingContext context)
    {
        if (Kind != NamedTypeKind.Enum)
            return [];

        SourceEnumOrdinalSymbol? current = null;

        using var declarationList = _mergedDeclaration
            .Declarations.AsValueEnumerable()
            .Select(d => (EnumDeclarationSyntax)d.SyntaxReference.Syntax)
            .Where(d => d.Ordinals.Count > 0)
            .ToArrayPool();

        if (declarationList.Size == 0)
            return [];

        var requireExplicitFirstElement = declarationList.Size > 1;

        var builder = ImmutableArray.CreateBuilder<EnumOrdinalSymbol>();
        foreach (var declaration in declarationList.Span)
        {
            if (requireExplicitFirstElement && declaration.Ordinals[0].Value is null)
            {
                context.ReportDiagnostic(
                    Diagnostic.PartialEnumOrdinalMustBeExplicit(declaration.Ordinals[0].Location)
                );
            }

            foreach (var ordinal in declaration.Ordinals)
            {
                current = new SourceEnumOrdinalSymbol(
                    DeclaringCompilation,
                    ordinal,
                    PublicSymbol,
                    current
                );
                builder.Add(current.PublicSymbol);
            }
        }

        return builder.DrainToImmutable();
    }

    public ImmutableArray<ParameterSymbol> AttributeParameters
    {
        get
        {
            return LazySymbols.LazyCompute(
                ref field,
                this,
                static (@this, context) => @this.ComputeAttributeParameters(context),
                static (@this, context) => @this.AddDeclarationDiagnostics(context)
            );
        }
    }

    private ImmutableArray<ParameterSymbol> ComputeAttributeParameters(BindingContext context)
    {
        if (Kind != NamedTypeKind.Attribute)
            return [];

        var declarations = _mergedDeclaration.Declarations;
        Debug.Assert(declarations.Length == 1);
        var syntax = (AttributeDeclarationSyntax)declarations[0].SyntaxReference.Syntax;

        if (syntax.Parameters is null || syntax.Parameters.Parameters.Count == 0)
        {
            return [];
        }

        var seenDefaultValue = false;
        var builder = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (var parameter in syntax.Parameters.Parameters)
        {
            var name = parameter.Name.IdentifierName;

            var symbol = new SourceAttributeParameterSymbol(
                DeclaringCompilation,
                PublicSymbol,
                parameter
            ).PublicSymbol;
            builder.Add(symbol);

            if (symbol.HasDefaultValue)
            {
                seenDefaultValue = true;
            }
            else if (seenDefaultValue)
            {
                context.ReportDiagnostic(
                    Diagnostic.DefaultValueAfterNonDefaultValue(parameter.Location, name)
                );
            }
        }

        return builder.DrainToImmutable();
    }

    protected override ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    )
    {
        var builder = new Dictionary<string, ImmutableArray<Symbol>.Builder>();

        foreach (var symbol in _mergedDeclaration.Members.Select(BuildSymbol))
        {
            builder.GetOrAdd(symbol.Name, ImmutableArray.CreateBuilder<Symbol>).Add(symbol);
        }

        foreach (var ordinal in EnumOrdinals)
        {
            builder.GetOrAdd(ordinal.Name, ImmutableArray.CreateBuilder<Symbol>).Add(ordinal);
        }

        foreach (var parameter in AttributeParameters)
        {
            builder.GetOrAdd(parameter.Name, ImmutableArray.CreateBuilder<Symbol>).Add(parameter);
        }

        foreach (
            var syntax in _mergedDeclaration
                .Declarations.AsValueEnumerable()
                .SelectMany(x => GetSyntaxMembers(x).AsValueEnumerable())
        )
        {
            Symbol? symbol = syntax switch
            {
                TypeDeclarationSyntax => null,
                FieldDeclarationSyntax field => BuildSymbol(field),
                FunctionDeclarationSyntax function => BuildSymbol(function),
                ConstructorDeclarationSyntax constructor => BuildSymbol(constructor),
                _ => null,
            };
            if (symbol is null)
                continue;

            builder.GetOrAdd(symbol.Name, ImmutableArray.CreateBuilder<Symbol>).Add(symbol);
        }

        AddSynthesizedMembers(builder);

        return builder.ToImmutableDictionary(
            pair => pair.Key,
            pair => pair.Value.DrainToImmutable()
        );

        static SyntaxList<DeclarationSyntax> GetSyntaxMembers(SingleTypeDeclaration x)
        {
            return x.SyntaxReference.Syntax switch
            {
                ClassDeclarationSyntax cu => cu.Members,
                _ => new SyntaxList<DeclarationSyntax>(),
            };
        }
    }

    private Symbol BuildSymbol(MergedTypeDeclaration declaration)
    {
        return new SourceNamedTypeSymbol(
            DeclaringCompilation,
            declaration,
            PublicSymbol
        ).PublicSymbol;
    }

    private VariableSymbol BuildSymbol(FieldDeclarationSyntax fieldDeclaration)
    {
        return new SourceFieldSymbol(
            DeclaringCompilation,
            PublicSymbol,
            fieldDeclaration
        ).PublicSymbol;
    }

    private FunctionSymbol BuildSymbol(FunctionDeclarationSyntax functionDeclaration)
    {
        return new SourceFunctionSymbol(
            DeclaringCompilation,
            PublicSymbol,
            functionDeclaration
        ).PublicSymbol;
    }

    private FunctionSymbol BuildSymbol(ConstructorDeclarationSyntax constructorDeclaration)
    {
        return new SourceConstructorSymbol(
            DeclaringCompilation,
            PublicSymbol,
            constructorDeclaration
        ).PublicSymbol;
    }

    private void AddSynthesizedMembers(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        if (Kind == NamedTypeKind.Class)
        {
            if (!nameToMembersMap.ContainsKey(CommonNames.Ctor))
            {
                TryAddDefaultConstructor(nameToMembersMap);
            }
        }
    }

    private void TryAddDefaultConstructor(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        if (SpecialType is SpecialType.Void or SpecialType.Str)
            return;

        foreach (
            var field in nameToMembersMap
                .AsValueEnumerable()
                .SelectMany(x => x.Value.AsValueEnumerable())
                .OfType<VariableSymbol>()
                .Where(v => v.IsInstance)
        )
        {
            if (field is { HasInitializer: false, Type.IsDefaultConstructible: false })
                return;
        }

        var builder = ImmutableArray.CreateBuilder<Symbol>();
        builder.Add(
            new SynthesizedDefaultConstructor(DeclaringCompilation, PublicSymbol).PublicSymbol
        );
        nameToMembersMap.Add(CommonNames.Ctor, builder);
    }

    private void LazyMemberChecks()
    {
        if (CompletionState.IsComplete(CompletionPart.FinishChecks))
            return;

        using var scope = _memberChecksLock.EnterScope();
        if (!CompletionState.MarkPartComplete(CompletionPart.StartChecks))
            return;

        using var context = BindingContext.Create();
        try
        {
            ValidateEnumOrdinals(context);
            foreach (var (name, members) in GetNameToMembersMap())
            {
                ValidateMembers(name, members, context);
            }
            AddDeclarationDiagnostics(context);
        }
        finally
        {
            CompletionState.MarkPartComplete(CompletionPart.FinishChecks);
        }
    }

    private void ValidateEnumOrdinals(BindingContext context)
    {
        if (Kind != NamedTypeKind.Enum)
            return;

        var ordinals = EnumOrdinals;
        if (ordinals.IsEmpty)
            return;

        var seenValues = new HashSet<ConstantValue>();

        foreach (var ordinal in ordinals)
        {
            if (seenValues.Add(ordinal.Value))
                continue;

            var syntax = (EnumOrdinalDeclarationSyntax)
                ordinal.DeclaringSyntaxReferences.Single().Syntax;
            var location = syntax.Value?.Location ?? syntax.Location;
            context.ReportDiagnostic(
                Diagnostic.EnumOrdinalNotUnique(location, ordinal.Value.ToString())
            );
        }
    }

    public bool IsDynamicallySized => SpecialType == SpecialType.Str;

    public NamedTypeSymbol? EnumUnderlyingType
    {
        get
        {
            if (Kind != NamedTypeKind.Enum)
                return null;

            if (field is not null)
                return field;

            using var context = BindingContext.Create();
            if (
                Interlocked.CompareExchange(ref field, ComputeUnderlyingType(context), null)
                is not null
            )
                return field;

            AddDeclarationDiagnostics(context);
            CompletionState.MarkPartComplete(CompletionPart.EnumUnderlyingType);
            return field;
        }
    }

    private NamedTypeSymbol ComputeUnderlyingType(BindingContext context)
    {
        // Fallback is i32 to line up with what C, C++, and C# use for defaults
        const SpecialType defaultUnderlyingType = SpecialType.I32;

        // TODO: Currently we don't actually have a syntax for the underlying type, once we do, we need to parse that
        NamedTypeSymbol? underlyingType = null;
        Location? primaryLocation = null;
        List<Location>? secondaryLocations = null;
        foreach (
            var syntax in _mergedDeclaration
                .Declarations.AsValueEnumerable()
                .Select(declaration => (EnumDeclarationSyntax)declaration.SyntaxReference.Syntax)
        )
        {
            if (syntax.BaseList is null)
                continue;

            var factory = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree);
            var binder = factory.GetBinder(syntax);
            var baseType = syntax.BaseList.Types[0];
            primaryLocation ??= baseType.Location;

            var resolvedType = (NamedTypeSymbol)binder.ResolveType(baseType, context);
            // If the first type is an interface or a concept then we don't care about it because that's not the
            // underlying type of an enum.
            //
            // Note: enums are not allow to implement interfaces, but that is a different story.
            if (resolvedType.Kind is NamedTypeKind.Interface or NamedTypeKind.Concept)
                continue;
            if (underlyingType is null)
            {
                underlyingType = resolvedType;
            }
            else
            {
                secondaryLocations ??= [];
                secondaryLocations.Add(baseType.Location);
            }
        }

        if (underlyingType is null)
            return DeclaringCompilation.GetSpecialType(defaultUnderlyingType);

        Debug.Assert(primaryLocation is not null);
        if (secondaryLocations is not null)
        {
            context.ReportDiagnostic(
                Diagnostic.BaseTypeMismatch(
                    primaryLocation,
                    secondaryLocations,
                    PublicSymbol.ToDisplayString()
                )
            );
        }

        if (
            underlyingType.SpecialType.IsInteger
            && underlyingType.SpecialType is not SpecialType.I128 and not SpecialType.U128
        )
        {
            return underlyingType;
        }

        context.ReportDiagnostic(
            Diagnostic.InvalidEnumBaseType(primaryLocation, PublicSymbol.ToDisplayString())
        );
        return DeclaringCompilation.GetSpecialType(defaultUnderlyingType);
    }

    public bool IsDefaultConstructible
    {
        get
        {
            if (_isDefaultConstructible.HasValue)
                return _isDefaultConstructible.Value;

            var isDefaultConstructible = ComputeIsDefaultConstructible();
            Interlocked.Exchange(
                ref _isDefaultConstructible,
                isDefaultConstructible.ToThreeState()
            );
            return isDefaultConstructible;
        }
    }

    private bool ComputeIsDefaultConstructible()
    {
        return Kind switch
        {
            NamedTypeKind.Class => GetMembers(CommonNames.Ctor)
                .OfType<FunctionSymbol>()
                .Any(f => f.Parameters.Length == 0),
            NamedTypeKind.Interface => false,
            NamedTypeKind.Concept => false,
            NamedTypeKind.Enum => EnumOrdinals.Any(o =>
                o.Value is IntegerConstant { IsZero: true }
            ),
            NamedTypeKind.Variant => throw new NotImplementedException(),
            NamedTypeKind.Attribute => false,
            _ => throw new InvalidOperationException("Unknown named type kind"),
        };
    }

    public ImmutableArray<AttributeData> GetAttributes()
    {
        if (!_attributes.IsDefault)
            return _attributes;

        using var context = BindingContext.Create();
        if (
            !ImmutableInterlocked.InterlockedInitialize(ref _attributes, ComputeAttributes(context))
        )
            return _attributes;

        AddDeclarationDiagnostics(context);
        CompletionState.MarkPartComplete(CompletionPart.Attributes);
        return _attributes;
    }

    private ImmutableArray<AttributeData> ComputeAttributes(BindingContext context)
    {
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        return _mergedDeclaration
            .Declarations.AsValueEnumerable()
            .SelectMany(x =>
            {
                var semanticModel = compilation.GetSemanticModel(x.SyntaxReference.SyntaxTree);
                var typeSyntax = (TypeDeclarationSyntax)x.SyntaxReference.Syntax;
                var binder = semanticModel.GetBinder(typeSyntax);
                return binder.BindAttributes(typeSyntax.Attributes, context).AsValueEnumerable();
            })
            .ToImmutableArray();
    }

    public override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(PublicSymbol) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = CompletionState.NextIncompletePart;
            switch (incompletePart)
            {
                case CompletionPart.Attributes:
                    _ = GetAttributes();
                    break;
                case CompletionPart.EnumUnderlyingType:
                    _ = EnumUnderlyingType;
                    break;
                case CompletionPart.Members:
                    _ = GetNameToMembersMap();
                    break;
                case CompletionPart.MembersCompleted:
                {
                    var allCompleted = true;

                    foreach (var member in GetMembers())
                    {
                        ForceCompleteMemberConditionally(
                            location,
                            filter,
                            member,
                            cancellationToken
                        );
                        allCompleted &= member.IsComplete(CompletionPart.Members);
                    }

                    if (allCompleted)
                    {
                        CompletionState.MarkPartComplete(CompletionPart.MembersCompleted);
                        break;
                    }
                    var allParts =
                        location is null && filter is null
                            ? CompletionPart.TypeAll
                            : CompletionPart.TypeAll & ~CompletionPart.MembersCompleted;
                    CompletionState.MarkPartComplete(allParts);
                    return;
                }
                case CompletionPart.StartChecks or CompletionPart.FinishChecks:
                    LazyMemberChecks();
                    break;
                case CompletionPart.None:
                    return;
                default:
                    // Any other values are for other kinds of symbols
                    CompletionState.MarkPartComplete(CompletionPart.All & ~CompletionPart.TypeAll);
                    break;
            }

            CompletionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }
}
