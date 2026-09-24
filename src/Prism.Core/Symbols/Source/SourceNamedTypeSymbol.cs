// @file SourceNamedTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Binding;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Symbols.Source;

internal abstract class SourceNamedTypeSymbol : NamedTypeSymbol
{
    private readonly MergedTypeDeclaration _mergedDeclaration;
    private ImmutableArray<Symbol> _members;
    private ImmutableDictionary<string, ImmutableArray<Symbol>>? _nameToMembersMap;
    private SymbolCompletionState _completionState;
    private ImmutableArray<AttributeData> _attributes;

    internal SourceNamedTypeSymbol(MergedTypeDeclaration mergedDeclaration, Symbol containingSymbol)
        : base(mergedDeclaration.Name, containingSymbol, GetTypeKind(mergedDeclaration))
    {
        _mergedDeclaration = mergedDeclaration;
    }

    private static NamedTypeKind GetTypeKind(MergedTypeDeclaration declaration)
    {
        return declaration.Kind switch
        {
            DeclarationKind.Namespace => throw new InvalidOperationException(
                "Type kind cannot be a namespace"
            ),
            DeclarationKind.Attribute => NamedTypeKind.Attribute,
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
    public override DeclaredVisibility DeclaredVisibility =>
        DeclaredVisibility.FromDeclarationModifiers(_mergedDeclaration.Modifiers);

    public sealed override ImmutableArray<Symbol> GetMembers()
    {
        if (!_members.IsDefault)
            return _members;

        ImmutableInterlocked.InterlockedCompareExchange(ref _members, ComputeMembers(), default);
        return _members;
    }

    public sealed override ImmutableArray<Symbol> GetMembers(string name)
    {
        return GetNameToMembersMap().GetValueOrDefault(name, []);
    }

    private ImmutableArray<Symbol> ComputeMembers()
    {
        var nameToMembersMap = GetNameToMembersMap();
        if (nameToMembersMap.Count == 0)
            return [];

        var members = nameToMembersMap
            .Values.AsValueEnumerable()
            .SelectMany(v => v.AsValueEnumerable())
            .ToArray();
        var compilation = DeclaringCompilation;
        Debug.Assert(compilation is not null);
        Array.Sort(members, SymbolLocationComparer.Get(compilation));
        return ImmutableCollectionsMarshal.AsImmutableArray(members);
    }

    private ImmutableDictionary<string, ImmutableArray<Symbol>> GetNameToMembersMap()
    {
        if (_nameToMembersMap is not null)
            return _nameToMembersMap;

        using var context = BindingContext.Create();
        if (
            Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(context), null)
            is not null
        )
            return _nameToMembersMap;

        AddDeclarationDiagnostics(context);
        _completionState.MarkPartComplete(CompletionPart.Members);
        return _nameToMembersMap;
    }

    private ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    )
    {
        var output = MakeNameToMembersMapCore(context);

        foreach (var (name, members) in output)
        {
            ValidateMembers(name, members, context);
        }

        return output;
    }

    protected abstract ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMapCore(
        BindingContext context
    );

    public sealed override bool IsDynamicallySized => false;

    public override ImmutableArray<AttributeData> GetAttributes()
    {
        if (!_attributes.IsDefault)
            return _attributes;

        using var context = BindingContext.Create();
        if (
            !ImmutableInterlocked.InterlockedInitialize(ref _attributes, ComputeAttributes(context))
        )
            return _attributes;

        AddDeclarationDiagnostics(context);
        _completionState.MarkPartComplete(CompletionPart.Attributes);
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

    internal sealed override bool NeedsCompletion => true;

    internal sealed override void ForceComplete(
        SourceLocation? location,
        Predicate<Symbol>? filter,
        CancellationToken cancellationToken
    )
    {
        if (filter?.Invoke(this) == false)
            return;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var incompletePart = _completionState.NextIncompletePart;
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
                        _completionState.MarkPartComplete(CompletionPart.MembersCompleted);
                        break;
                    }
                    var allParts =
                        location is null && filter is null
                            ? CompletionPart.TypeAll
                            : CompletionPart.TypeAll & ~CompletionPart.MembersCompleted;
                    _completionState.MarkPartComplete(allParts);
                    return;
                }
                case CompletionPart.None:
                    return;
                default:
                    // Any other values are for other kinds of symbols
                    _completionState.MarkPartComplete(CompletionPart.All & ~CompletionPart.TypeAll);
                    break;
            }

            _completionState.WaitPartComplete(incompletePart, cancellationToken);
        }
    }

    internal sealed override bool IsComplete(CompletionPart part)
    {
        return _completionState.IsComplete(part);
    }
}
