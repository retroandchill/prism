// @file cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Semantic.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic;

internal sealed class BuiltInTypeSet(Compilation compilation)
{
    public NamedTypeSymbol Void { get; } = new(BuiltInType.Void, compilation);

    public NamedTypeSymbol Bool { get; } = new(BuiltInType.Bool, compilation);

    public NamedTypeSymbol I8 { get; } = new(BuiltInType.I8, compilation);

    public NamedTypeSymbol I16 { get; } = new(BuiltInType.I16, compilation);

    public NamedTypeSymbol I32 { get; } = new(BuiltInType.I32, compilation);

    public NamedTypeSymbol I64 { get; } = new(BuiltInType.I64, compilation);

    public NamedTypeSymbol I128 { get; } = new(BuiltInType.I128, compilation);

    // ReSharper disable once InconsistentNaming
    public NamedTypeSymbol ISize { get; } = new(BuiltInType.ISize, compilation);

    public NamedTypeSymbol U8 { get; } = new(BuiltInType.U8, compilation);

    public NamedTypeSymbol U16 { get; } = new(BuiltInType.U16, compilation);

    public NamedTypeSymbol U32 { get; } = new(BuiltInType.U32, compilation);

    public NamedTypeSymbol U64 { get; } = new(BuiltInType.U64, compilation);

    public NamedTypeSymbol U128 { get; } = new(BuiltInType.U128, compilation);

    public NamedTypeSymbol USize { get; } = new(BuiltInType.USize, compilation);

    public NamedTypeSymbol F16 { get; } = new(BuiltInType.F16, compilation);

    public NamedTypeSymbol F32 { get; } = new(BuiltInType.F32, compilation);

    public NamedTypeSymbol F64 { get; } = new(BuiltInType.F64, compilation);

    public NamedTypeSymbol Char { get; } = new(BuiltInType.Char, compilation);

    public NamedTypeSymbol Char16 { get; } = new(BuiltInType.Char16, compilation);

    public NamedTypeSymbol Rune { get; } = new(BuiltInType.Rune, compilation);

    public NamedTypeSymbol Str { get; } = new(BuiltInType.Str, compilation);

    public NamedTypeSymbol GetSymbol(BuiltInType type)
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

    public NamedTypeSymbol GetTypeSymbol(IntegerSuffix suffix)
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

    public NamedTypeSymbol GetTypeSymbol(FloatSuffix suffix)
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
