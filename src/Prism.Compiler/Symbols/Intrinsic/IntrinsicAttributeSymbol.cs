// @file IntrinsicAttributeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;
using ZLinq;

namespace Prism.Compiler.Symbols.Intrinsic;

internal sealed class IntrinsicAttributeSymbol : INamedTypeSymbolData, IMemberContainerSymbolData
{
    internal IntrinsicAttributeSymbol(string name, Symbol containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public string Name { get; }

    public Symbol ContainingSymbol { get; }

    public NamedTypeKind Kind => NamedTypeKind.Attribute;

    private ImmutableDictionary<string, ImmutableArray<Symbol>> _membersByName = [];
    internal ImmutableArray<Symbol> Members
    {
        get;
        set
        {
            if (!field.IsDefault)
                throw new InvalidOperationException("Members cannot be set more than once.");

            field = value;
            _membersByName = field
                .AsValueEnumerable()
                .GroupBy(x => x.Name)
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableArray());
        }
    }

    public ImmutableArray<Symbol> GetMembers()
    {
        return Members;
    }

    public ImmutableArray<Symbol> GetMembers(string name)
    {
        return _membersByName.GetValueOrDefault(name, []);
    }

    public bool IsDynamicallySized => false;
    public bool IsDefaultConstructible => false;
}
