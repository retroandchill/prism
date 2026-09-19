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

    protected abstract TypeDeclarationSyntax Syntax { get; }

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedCompareExchange(
                ref field,
                [Syntax.Identifier.Location],
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
                [new SyntaxReference(Syntax)],
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

    protected abstract ImmutableDictionary<string, ImmutableArray<Symbol>> MakeNameToMembersMap(
        BindingContext context
    );

    public sealed override bool IsDynamicallySized => false;
}
