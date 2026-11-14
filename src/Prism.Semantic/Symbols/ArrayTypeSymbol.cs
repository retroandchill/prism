// @file ArrayTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class ArrayTypeSymbol : TypeSymbol
{
    internal ArrayTypeSymbol(TypeSymbol elementType, ulong? size = null)
        : base(new DataInternal(elementType, size)) { }

    private new DataInternal Data => Unsafe.As<DataInternal>(base.Data);

    public TypeSymbol ElementType => Data.ElementType;

    public ulong? Size => Data.Size;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        ElementType.WriteDisplayString(ref builder);
        builder.Append('[');
        if (Size is { } size)
        {
            builder.Append(size);
        }
        builder.Append(']');
    }

    public override bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return Equals(other as ArrayTypeSymbol, kind);
    }

    private bool Equals(ArrayTypeSymbol? other, SymbolCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        return other is not null
            && ElementType.Equals(other.ElementType, kind)
            && Size == other.Size;
    }

    public override int GetHashCode(SymbolCompareKind kind)
    {
        return HashCode.Combine(ElementType.GetHashCode(kind), Size);
    }

    private sealed record DataInternal(TypeSymbol ElementType, ulong? Size) : ITypeSymbolData
    {
        public string Name => Size is { } s ? $"{ElementType.Name}[{s}]" : $"{ElementType.Name}[]";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => Size is null;
        public bool IsDefaultConstructible =>
            Size is not null && (Size == 0 || ElementType.IsDefaultConstructible);
    }
}
