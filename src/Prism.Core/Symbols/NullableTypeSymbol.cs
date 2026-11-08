// @file NullableTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public sealed class NullableTypeSymbol : TypeSymbol
{
    internal NullableTypeSymbol(TypeSymbol elementType)
    {
        ElementType = elementType;
    }

    public override string Name => $"{ElementType.Name}?";

    public override Symbol? ContainingSymbol => null;

    public TypeSymbol ElementType { get; }

    public override bool IsDynamicallySized => false;
    public override bool IsDefaultConstructible => true;

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override void WriteDisplayString(TextWriter writer)
    {
        ElementType.WriteDisplayString(writer);
        writer.Write('?');
    }

    public override bool Equals(TypeSymbol other, TypeCompareKind kind)
    {
        return Equals(other as NullableTypeSymbol, kind);
    }

    private bool Equals(NullableTypeSymbol? other, TypeCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        return other is not null && ElementType.Equals(other.ElementType, kind);
    }

    public override int GetHashCode(TypeCompareKind kind) =>
        HashCode.Combine(ElementType.GetHashCode(kind));
}
