// @file ReferenceTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Cysharp.Text;
using Prism.Configuration;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class ReferenceTypeSymbol : TypeSymbol
{
    internal ReferenceTypeSymbol(
        AssemblySymbol declaringAssembly,
        TypeSymbol referencedType,
        bool isMutable
    )
        : base(
            new DataInternal(referencedType, isMutable, declaringAssembly.TargetInfo.PointerWidth)
        ) { }

    private new DataInternal Data => (DataInternal)base.Data;

    public TypeSymbol ReferencedType => Data.ReferencedType;

    public bool IsMutable => Data.IsMutable;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        ReferencedType.WriteDisplayString(ref builder);
        builder.Append(IsMutable ? " mutable&" : "&");
    }

    public override TypeLayout GetLayout(CancellationToken cancellationToken = default)
    {
        if (ReferencedType.IsDynamicallySized)
        {
            return Data.PointerWidth switch
            {
                PointerWidth.X32 => Slice32,
                PointerWidth.X64 => Slice64,
                _ => throw new ArgumentException("Invalid pointer width"),
            };
        }

        return Data.PointerWidth switch
        {
            PointerWidth.X32 => Reference32,
            PointerWidth.X64 => Reference64,
            _ => throw new ArgumentException("Invalid pointer width"),
        };
    }

    public override bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return Equals(other as ReferenceTypeSymbol, kind);
    }

    private bool Equals(ReferenceTypeSymbol? other, SymbolCompareKind kind)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other is null || !ReferencedType.Equals(other.ReferencedType, kind))
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

    private sealed record DataInternal(
        TypeSymbol ReferencedType,
        bool IsMutable,
        PointerWidth PointerWidth
    ) : ITypeSymbolData
    {
        public string Name => $"{ReferencedType.Name}{(IsMutable ? " mutable" : "")}&";
        public Symbol? ContainingSymbol => null;
        public bool IsDynamicallySized => false;
        public bool IsDefaultConstructible => false;
    }

    private static readonly PointerLayout Reference32 = new(PointerWidth.X32, true);
    private static readonly PointerLayout Reference64 = new(PointerWidth.X64, true);

    private static readonly StructLayout Slice32 = MakeSliceLayout(PointerWidth.X32);
    private static readonly StructLayout Slice64 = MakeSliceLayout(PointerWidth.X64);

    private static StructLayout MakeSliceLayout(PointerWidth width)
    {
        var pointer = width switch
        {
            PointerWidth.X32 => Reference32,
            PointerWidth.X64 => Reference64,
            _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
        };

        var size = GetIntegerLayout(width.BitWidth, false);

        Debug.Assert(pointer.Alignment == size.Alignment);
        var typeLayoutInfo = new TypeLayoutInfo(pointer.Size + size.Size, pointer.Alignment);
        return new StructLayout(
            typeLayoutInfo,
            [new StructField("data", pointer, 0), new StructField("size", size, pointer.Size)]
        );
    }
}
