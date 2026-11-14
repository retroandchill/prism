// @file NullableTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class NullableTypeSymbol : TypeSymbol
{
    internal NullableTypeSymbol(TypeSymbol elementType)
        : base(new DataInternal(elementType)) { }

    private new DataInternal Data => Unsafe.As<DataInternal>(base.Data);

    public TypeSymbol ElementType => Data.ElementType;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        ElementType.WriteDisplayString(ref builder);
        builder.Append('?');
    }

    public override bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return Equals(other as NullableTypeSymbol, kind);
    }

    private bool Equals(NullableTypeSymbol? other, SymbolCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        return other is not null && ElementType.Equals(other.ElementType, kind);
    }

    public override int GetHashCode(SymbolCompareKind kind) =>
        HashCode.Combine(ElementType.GetHashCode(kind));

    private sealed record DataInternal(TypeSymbol ElementType) : ITypeSymbolData
    {
        public string Name => $"{ElementType.Name}?";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => false;
        public bool IsDefaultConstructible => true;
    }
}
