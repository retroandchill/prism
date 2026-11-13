// @file SourceNamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Prism.Compiler.Binding;
using Prism.Compiler.Declarations;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;
using Prism.Utils;
using ZLinq;

namespace Prism.Compiler.Symbols.Source;

internal abstract class SourceNamedTypeSymbol
    : SourceMemberContainerSymbol,
        INamedTypeSymbolData,
        IAttributeHolderSymbolData,
        IVisibilityGuardedSymbolData
{
    private readonly MergedTypeDeclaration _mergedDeclaration;
    private ImmutableArray<AttributeData> _attributes;
    private readonly Lock _memberChecksLock = new();

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

    public sealed override string Name => _mergedDeclaration.Name;

    public sealed override Symbol ContainingSymbol { get; }

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

    protected override ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    )
    {
        var builder = new Dictionary<string, ImmutableArray<Symbol>.Builder>();

        foreach (var symbol in _mergedDeclaration.Members.Select(BuildSymbol))
        {
            builder.GetOrAdd(symbol.Name, ImmutableArray.CreateBuilder<Symbol>).Add(symbol);
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

        SyntaxList<DeclarationSyntax> GetSyntaxMembers(SingleTypeDeclaration x)
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
        return declaration.Kind switch
        {
            DeclarationKind.Namespace => throw new InvalidOperationException(
                "Namespace declaration cannot be built as a type"
            ),
            DeclarationKind.Class => new SourceClassSymbol(
                DeclaringCompilation,
                declaration,
                PublicSymbol
            ).PublicSymbol,
            DeclarationKind.Attribute => new SourceAttributeSymbol(
                DeclaringCompilation,
                declaration,
                PublicSymbol
            ).PublicSymbol,
            _ => throw new InvalidOperationException("Unknown declaration kind"),
        };
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

    protected virtual void AddSynthesizedMembers(
        Dictionary<string, ImmutableArray<Symbol>.Builder> nameToMembersMap
    )
    {
        // By default, we don't need any
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

    public bool IsDynamicallySized => SpecialType == SpecialType.Str;

    public abstract bool IsDefaultConstructible { get; }

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

    public sealed override void ForceComplete(
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
