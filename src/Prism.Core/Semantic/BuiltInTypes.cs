// @file BuiltInTypes.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public static class BuiltInTypes
{
    private static readonly NamedTypeSymbol Void = new()
    {
        Name = KnownName.Void,
        BuiltInType = BuiltInType.Void,
    };

    private static readonly NamedTypeSymbol Bool = new()
    {
        Name = KnownName.Bool,
        BuiltInType = BuiltInType.Bool,
    };

    private static readonly NamedTypeSymbol I8 = new()
    {
        Name = KnownName.I8,
        BuiltInType = BuiltInType.I8,
    };

    private static readonly NamedTypeSymbol I16 = new()
    {
        Name = KnownName.I16,
        BuiltInType = BuiltInType.I16,
    };

    private static readonly NamedTypeSymbol I32 = new()
    {
        Name = KnownName.I32,
        BuiltInType = BuiltInType.I32,
    };

    private static readonly NamedTypeSymbol I64 = new()
    {
        Name = KnownName.I64,
        BuiltInType = BuiltInType.I64,
    };

    private static readonly NamedTypeSymbol I128 = new()
    {
        Name = KnownName.I128,
        BuiltInType = BuiltInType.I128,
    };

    // ReSharper disable once InconsistentNaming
    private static readonly NamedTypeSymbol ISize = new()
    {
        Name = KnownName.ISize,
        BuiltInType = BuiltInType.ISize,
    };

    private static readonly NamedTypeSymbol U8 = new()
    {
        Name = KnownName.U8,
        BuiltInType = BuiltInType.U8,
    };

    private static readonly NamedTypeSymbol U16 = new()
    {
        Name = KnownName.U16,
        BuiltInType = BuiltInType.U16,
    };

    private static readonly NamedTypeSymbol U32 = new()
    {
        Name = KnownName.U32,
        BuiltInType = BuiltInType.U32,
    };

    private static readonly NamedTypeSymbol U64 = new()
    {
        Name = KnownName.U64,
        BuiltInType = BuiltInType.U64,
    };

    private static readonly NamedTypeSymbol U128 = new()
    {
        Name = KnownName.U128,
        BuiltInType = BuiltInType.U128,
    };

    private static readonly NamedTypeSymbol USize = new()
    {
        Name = KnownName.USize,
        BuiltInType = BuiltInType.USize,
    };

    private static readonly NamedTypeSymbol F16 = new()
    {
        Name = KnownName.F16,
        BuiltInType = BuiltInType.F16,
    };

    private static readonly NamedTypeSymbol F32 = new()
    {
        Name = KnownName.F32,
        BuiltInType = BuiltInType.F32,
    };

    private static readonly NamedTypeSymbol F64 = new()
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

    private static readonly NamedTypeSymbol Str = new()
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
    }
}
