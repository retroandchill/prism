// @file ReferenceTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;

namespace Prism.Semantic;

public sealed class ReferenceTypeSymbol : TypeSymbol
{
    internal ReferenceTypeSymbol(TypeSymbol referencedType, bool isMutable)
        : base(new DataInternal(referencedType, isMutable)) { }

    private new DataInternal Data => (DataInternal)base.Data;

    public TypeSymbol ReferencedType => Data.ReferencedType;

    public bool IsMutable => Data.IsMutable;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        ReferencedType.WriteDisplayString(ref builder);
        builder.Append(IsMutable ? " mutable&" : "&");
    }

    public override bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return Equals(other as ReferenceTypeSymbol, kind);
    }

    private bool Equals(ReferenceTypeSymbol? other, SymbolCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || ReferencedType.Equals(other.ReferencedType, kind))
        {
            return false;
        }

        if (kind.HasFlag(SymbolCompareKind.IgnoreMutability))
            return true;

        return IsMutable == other.IsMutable;
    }

    public override int GetHashCode(SymbolCompareKind kind)
    {
        var hashCode = new HashCode();

        hashCode.Add(ReferencedType.GetHashCode(kind));

        if (!kind.HasFlag(SymbolCompareKind.IgnoreMutability))
        {
            hashCode.Add(IsMutable);
        }

        return hashCode.ToHashCode();
    }

    private sealed record DataInternal(TypeSymbol ReferencedType, bool IsMutable) : ITypeSymbolData
    {
        public string Name => $"{ReferencedType.Name}{(IsMutable ? " mutable" : "")}&";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => false;
        public bool IsDefaultConstructible => false;
    }
}
