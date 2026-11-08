// @file ArrayTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

public sealed class ArrayTypeSymbol : TypeSymbol
{
    internal ArrayTypeSymbol(TypeSymbol elementType, ulong? size = null)
    {
        ElementType = elementType;
        Size = size;
    }

    public override string Name =>
        Size is { } s ? $"{ElementType.Name}[{s}]" : $"{ElementType.Name}[]";

    public override Symbol? ContainingSymbol => null;

    public TypeSymbol ElementType { get; }

    public ulong? Size { get; }

    [MemberNotNullWhen(false, nameof(Size))]
    public override bool IsDynamicallySized => Size is null;

    public override bool IsDefaultConstructible =>
        Size is not null && (Size == 0 || ElementType.IsDefaultConstructible);

    public override ImmutableArray<Location> Locations => [];

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override void WriteDisplayString(TextWriter writer)
    {
        ElementType.WriteDisplayString(writer);
        writer.Write('[');
        if (Size is { } size)
        {
            writer.Write(size);
        }
        writer.Write(']');
    }

    public override bool Equals(TypeSymbol other, TypeCompareKind kind)
    {
        return Equals(other as ArrayTypeSymbol, kind);
    }

    private bool Equals(ArrayTypeSymbol? other, TypeCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        return other is not null
            && ElementType.Equals(other.ElementType, kind)
            && Size == other.Size;
    }

    public override int GetHashCode(TypeCompareKind kind)
    {
        return HashCode.Combine(ElementType.GetHashCode(kind), Size);
    }
}
