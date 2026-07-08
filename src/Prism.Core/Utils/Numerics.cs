// @file NumericParsing.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using Prism.Core.Semantic;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Syntax;
using Singulink.Numerics;

namespace Prism.Core.Utils;

public static class Numerics
{
    private static readonly ImmutableArray<(string Suffix, IntegerSuffix Type)> IntegerSuffixes =
    [
        ("i8", IntegerSuffix.I8),
        ("i16", IntegerSuffix.I16),
        ("i32", IntegerSuffix.I32),
        ("i64", IntegerSuffix.I64),
        ("i128", IntegerSuffix.I128),
        ("iz", IntegerSuffix.ISize),
        ("u8", IntegerSuffix.U8),
        ("u16", IntegerSuffix.U16),
        ("u32", IntegerSuffix.U32),
        ("u64", IntegerSuffix.U64),
        ("u128", IntegerSuffix.U128),
        ("uz", IntegerSuffix.USize),
    ];

    private static readonly ImmutableArray<(string Suffix, FloatSuffix Type)> FloatSuffixes =
    [
        ("f16", FloatSuffix.F16),
        ("f32", FloatSuffix.F32),
        ("f64", FloatSuffix.F64),
    ];

    public static (IntegerBase Base, BigInteger Value, IntegerSuffix Suffix) ParseInteger(
        ReadOnlySpan<char> literalValue
    )
    {
        IntegerSuffix suffix = IntegerSuffix.None;
        foreach (var (str, type) in IntegerSuffixes)
        {
            if (!literalValue.EndsWith(str, StringComparison.Ordinal))
                continue;
            suffix = type;
            literalValue = literalValue[..^str.Length];
            break;
        }

        IntegerBase @base;
        NumberStyles styles;
        if (literalValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            @base = IntegerBase.Hex;
            literalValue = literalValue[2..];
            styles = NumberStyles.HexNumber;
        }
        else if (literalValue.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            @base = IntegerBase.Binary;
            literalValue = literalValue[2..];
            styles = NumberStyles.BinaryNumber;
        }
        else
        {
            @base = IntegerBase.Decimal;
            styles = NumberStyles.Integer;
        }

        char[]? scratchBuffer = null;
        try
        {
            if (literalValue.Contains('_'))
            {
                scratchBuffer = ArrayPool<char>.Shared.Rent(literalValue.Length);

                var i = 0;
                foreach (var c in literalValue)
                {
                    if (c == '_')
                        continue;

                    scratchBuffer[i] = c;
                    i++;
                }

                literalValue = scratchBuffer.AsSpan(i + 1);
            }

            return (
                @base,
                BigInteger.Parse(literalValue, styles, CultureInfo.InvariantCulture),
                suffix
            );
        }
        finally
        {
            if (scratchBuffer is not null)
                ArrayPool<char>.Shared.Return(scratchBuffer);
        }
    }

    public static (BigDecimal Value, FloatSuffix Suffix) ParseFloat(ReadOnlySpan<char> literalValue)
    {
        var suffix = FloatSuffix.None;
        foreach (var (str, type) in FloatSuffixes)
        {
            if (!literalValue.EndsWith(str, StringComparison.Ordinal))
                continue;
            suffix = type;
            literalValue = literalValue[..^str.Length];
            break;
        }

        return (
            BigDecimal.Parse(literalValue, NumberStyles.Float, CultureInfo.InvariantCulture),
            suffix
        );
    }
}
