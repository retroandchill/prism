// @file BuiltInTypes.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public static partial class BuiltInTypes
{
    private static readonly NamedTypeSymbol Void = new()
    {
        Name = KnownName.Void,
        BuiltInType = BuiltInType.Void,
    };

    public static readonly NamedTypeSymbol Bool = new()
    {
        Name = KnownName.Bool,
        BuiltInType = BuiltInType.Bool,
    };

    public static readonly NamedTypeSymbol I8 = new()
    {
        Name = KnownName.I8,
        BuiltInType = BuiltInType.I8,
    };

    public static readonly NamedTypeSymbol I16 = new()
    {
        Name = KnownName.I16,
        BuiltInType = BuiltInType.I16,
    };

    public static readonly NamedTypeSymbol I32 = new()
    {
        Name = KnownName.I32,
        BuiltInType = BuiltInType.I32,
    };

    public static readonly NamedTypeSymbol I64 = new()
    {
        Name = KnownName.I64,
        BuiltInType = BuiltInType.I64,
    };

    public static readonly NamedTypeSymbol I128 = new()
    {
        Name = KnownName.I128,
        BuiltInType = BuiltInType.I128,
    };

    // ReSharper disable once InconsistentNaming
    public static readonly NamedTypeSymbol ISize = new()
    {
        Name = KnownName.ISize,
        BuiltInType = BuiltInType.ISize,
    };

    public static readonly NamedTypeSymbol U8 = new()
    {
        Name = KnownName.U8,
        BuiltInType = BuiltInType.U8,
    };

    public static readonly NamedTypeSymbol U16 = new()
    {
        Name = KnownName.U16,
        BuiltInType = BuiltInType.U16,
    };

    public static readonly NamedTypeSymbol U32 = new()
    {
        Name = KnownName.U32,
        BuiltInType = BuiltInType.U32,
    };

    public static readonly NamedTypeSymbol U64 = new()
    {
        Name = KnownName.U64,
        BuiltInType = BuiltInType.U64,
    };

    public static readonly NamedTypeSymbol U128 = new()
    {
        Name = KnownName.U128,
        BuiltInType = BuiltInType.U128,
    };

    public static readonly NamedTypeSymbol USize = new()
    {
        Name = KnownName.USize,
        BuiltInType = BuiltInType.USize,
    };

    public static readonly NamedTypeSymbol F16 = new()
    {
        Name = KnownName.F16,
        BuiltInType = BuiltInType.F16,
    };

    public static readonly NamedTypeSymbol F32 = new()
    {
        Name = KnownName.F32,
        BuiltInType = BuiltInType.F32,
    };

    public static readonly NamedTypeSymbol F64 = new()
    {
        Name = KnownName.F64,
        BuiltInType = BuiltInType.F64,
    };

    private static readonly NamedTypeSymbol Char = new()
    {
        Name = KnownName.Char,
        BuiltInType = BuiltInType.Char,
    };

    private static readonly NamedTypeSymbol Char16 = new()
    {
        Name = KnownName.Char16,
        BuiltInType = BuiltInType.Char16,
    };

    private static readonly NamedTypeSymbol Rune = new()
    {
        Name = KnownName.Rune,
        BuiltInType = BuiltInType.Rune,
    };

    public static readonly NamedTypeSymbol Str = new()
    {
        Name = KnownName.Str,
        BuiltInType = BuiltInType.Str,
    };

    extension(BuiltInType type)
    {
        public NamedTypeSymbol GetSymbol()
        {
            return type switch
            {
                BuiltInType.Void => Void,
                BuiltInType.Bool => Bool,
                BuiltInType.I8 => I8,
                BuiltInType.I16 => I16,
                BuiltInType.I32 => I32,
                BuiltInType.I64 => I64,
                BuiltInType.I128 => I128,
                BuiltInType.ISize => ISize,
                BuiltInType.U8 => U8,
                BuiltInType.U16 => U16,
                BuiltInType.U32 => U32,
                BuiltInType.U64 => U64,
                BuiltInType.U128 => U128,
                BuiltInType.USize => USize,
                BuiltInType.F16 => F16,
                BuiltInType.F32 => F32,
                BuiltInType.F64 => F64,
                BuiltInType.Char => Char,
                BuiltInType.Char16 => Char16,
                BuiltInType.Rune => Rune,
                BuiltInType.Str => Str,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        public bool IsSignedInteger =>
            type
                is BuiltInType.I8
                    or BuiltInType.I16
                    or BuiltInType.I32
                    or BuiltInType.I64
                    or BuiltInType.I128
                    or BuiltInType.ISize;

        public bool IsUnsigned =>
            type
                is BuiltInType.U8
                    or BuiltInType.U16
                    or BuiltInType.U32
                    or BuiltInType.U64
                    or BuiltInType.U128
                    or BuiltInType.USize;

        public bool IsInteger => type.IsSignedInteger || type.IsUnsigned;

        public bool IsFloatingPoint =>
            type is BuiltInType.F16 or BuiltInType.F32 or BuiltInType.F64;

        public bool IsSigned => type.IsSignedInteger || type.IsFloatingPoint;

        public bool IsNumeric => type.IsInteger || type.IsFloatingPoint;

        public bool IsCharacter =>
            type is BuiltInType.Char or BuiltInType.Char16 or BuiltInType.Rune;
    }

    public static NamedTypeSymbol GetTypeSymbol(this IntegerSuffix suffix)
    {
        return suffix switch
        {
            IntegerSuffix.None => I32,
            IntegerSuffix.I8 => I8,
            IntegerSuffix.I16 => I16,
            IntegerSuffix.I32 => I32,
            IntegerSuffix.I64 => I64,
            IntegerSuffix.I128 => I128,
            IntegerSuffix.ISize => ISize,
            IntegerSuffix.U8 => U8,
            IntegerSuffix.U16 => U16,
            IntegerSuffix.U32 => U32,
            IntegerSuffix.U64 => U64,
            IntegerSuffix.U128 => U128,
            IntegerSuffix.USize => USize,
            _ => throw new ArgumentOutOfRangeException(nameof(suffix), suffix, null),
        };
    }

    public static NamedTypeSymbol GetTypeSymbol(this FloatSuffix suffix)
    {
        return suffix switch
        {
            FloatSuffix.None => F64,
            FloatSuffix.F16 => F16,
            FloatSuffix.F32 => F32,
            FloatSuffix.F64 => F64,
            _ => throw new ArgumentOutOfRangeException(nameof(suffix), suffix, null),
        };
    }
}
