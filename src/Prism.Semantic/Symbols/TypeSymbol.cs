// @file TypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public closed class TypeSymbol : MemberContainerSymbol
{
    private protected TypeSymbol(ITypeSymbolData data)
        : base(data) { }

    internal new ITypeSymbolData Data => Unsafe.As<ITypeSymbolData>(base.Data);

    public SpecialType SpecialType => Data.SpecialType;

    public bool IsErrorType => Data.IsErrorType;

    public bool IsVoid => SpecialType == SpecialType.Void;

    public bool IsDynamicallySized => Data.IsDynamicallySized;

    public bool IsDefaultConstructible => Data.IsDefaultConstructible;

    public abstract TypeLayout GetLayout(CancellationToken cancellationToken = default);

    public TypeSymbol UnwrapTypeAlias()
    {
        var typeSymbol = this;
        while (typeSymbol is TypeAliasSymbol typeAliasSymbol)
        {
            typeSymbol = typeAliasSymbol.AliasedType;
        }

        return typeSymbol;
    }

    public sealed override bool Equals(Symbol other, SymbolCompareKind kind)
    {
        return other switch
        {
            TypeAliasSymbol typeAliasSymbol => Equals(typeAliasSymbol.AliasedType, kind),
            TypeSymbol typeSymbol => Equals(typeSymbol, kind),
            _ => false,
        };
    }

    public virtual bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        return ReferenceEquals(this, other);
    }

    internal static readonly IntegerLayout BoolLayout = new(
        8,
        false,
        new BoundedRangeValidity(0, 1)
    );

    private static readonly ImmutableArray<IntegerLayout> SignedLayouts = MakeIntegerLayouts(
        128,
        true
    );
    private static readonly ImmutableArray<IntegerLayout> UnsignedLayouts = MakeIntegerLayouts(
        128,
        false
    );

    private const int ByteSize = (int)ScalarLayout.BitsPerByte;

    private static ImmutableArray<IntegerLayout> MakeIntegerLayouts(int maxBitSize, bool isSigned)
    {
        Debug.Assert(maxBitSize % ByteSize == 0);

        var size = maxBitSize / ByteSize;
        return
        [
            .. Enumerable
                .Range(0, size)
                .Select(bitSize => new IntegerLayout((ulong)(bitSize + 1) * ByteSize, isSigned)),
        ];
    }

    private protected static IntegerLayout GetIntegerLayout(int bitSize, bool isSigned)
    {
        Debug.Assert(bitSize % ByteSize == 0);
        var index = bitSize / ByteSize - 1;
        return isSigned ? SignedLayouts[index] : UnsignedLayouts[index];
    }
}
