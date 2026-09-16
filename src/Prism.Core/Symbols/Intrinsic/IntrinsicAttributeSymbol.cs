// @file IntrinsicAttributeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Configuration;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Symbols.Intrinsic;

internal sealed class IntrinsicAttributeSymbol : NamedTypeSymbol
{
    internal IntrinsicAttributeSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol, NamedTypeKind.Attribute) { }

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

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

    public override ImmutableArray<Symbol> GetMembers()
    {
        return Members;
    }

    public override ImmutableArray<Symbol> GetMembers(string name)
    {
        return _membersByName.GetValueOrDefault(name, []);
    }

    public override bool IsDynamicallySized => false;

    public override SizeAndAlignment GetSizeAndAlignment(CompilationSettings settings)
    {
        throw new NotSupportedException("Attributes are compile-time only artifacts.");
    }
}
