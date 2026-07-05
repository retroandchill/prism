// @file NumericParsing.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using Prism.Core.Semantic;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Utils;

public static class Numerics
{
    public static (UInt128 Value, bool Overflow) ParseInteger(ReadOnlySpan<char> literalValue)
    {
        bool overflow;
        UInt128 value;
        if (literalValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            overflow = UInt128.TryParse(
                literalValue[2..],
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out value
            );
            return (value, overflow);
        }

        if (literalValue.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            overflow = UInt128.TryParse(
                literalValue[2..],
                NumberStyles.BinaryNumber,
                CultureInfo.InvariantCulture,
                out value
            );
            return (value, overflow);
        }

        overflow = UInt128.TryParse(
            literalValue,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value
        );
        return (value, overflow);
    }

    public static (double Value, bool Overflow) ParseFloat(ReadOnlySpan<char> literalValue)
    {
        var overflow = double.TryParse(
            literalValue,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var value
        );
        return (value, overflow);
    }

    private static readonly ImmutableArray<(string Suffix, TypeSymbol Type)> IntegerSuffixes =
    [
        ("i8", BuiltInTypes.I8),
        ("i16", BuiltInTypes.I16),
        ("i32", BuiltInTypes.I32),
        ("i64", BuiltInTypes.I64),
        ("is", BuiltInTypes.ISize),
        ("u8", BuiltInTypes.U8),
        ("u16", BuiltInTypes.U16),
        ("u32", BuiltInTypes.U32),
        ("u64", BuiltInTypes.U64),
        ("us", BuiltInTypes.USize),
    ];

    public static (TypeSymbol Type, int SuffixLength) GetIntegerTypeFromSuffix(
        ReadOnlySpan<char> literal
    )
    {
        foreach (var (suffix, type) in IntegerSuffixes)
        {
            if (literal.EndsWith(suffix, StringComparison.Ordinal))
            {
                return (type, suffix.Length);
            }
        }

        return (BuiltInTypes.I32, 0);
    }

    private static readonly ImmutableArray<(string Suffix, TypeSymbol Type)> FloatSuffixes =
    [
        ("f16", BuiltInTypes.F16),
        ("f32", BuiltInTypes.F32),
        ("f64", BuiltInTypes.F64),
    ];

    public static (TypeSymbol Type, int SuffixLength) GetFloatTypeFromSuffix(
        ReadOnlySpan<char> literal
    )
    {
        foreach (var (suffix, type) in FloatSuffixes)
        {
            if (literal.EndsWith(suffix, StringComparison.Ordinal))
            {
                return (type, suffix.Length);
            }
        }

        return (BuiltInTypes.F64, 0);
    }
}
