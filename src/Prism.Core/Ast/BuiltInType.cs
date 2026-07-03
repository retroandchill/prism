// @file BuiltInType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;

namespace Prism.Core.Ast;

public enum BuiltInType
{
    Void,
    Bool,
    I8,
    I16,
    I32,
    I64,
    I128,

    // ReSharper disable once InconsistentNaming
    ISize,
    U8,
    U16,
    U32,
    U64,
    U128,
    USize,
    F16,
    F32,
    F64,
    Char,
    Char16,
    Rune,
    Str,
}

public static class BuiltInTypeExtensions
{
    extension(BuiltInType)
    {
        public static bool TryGet(TokenKind kind, out BuiltInType type)
        {
            (var success, type) = kind switch
            {
                TokenKind.Void => (true, BuiltInType.Void),
                TokenKind.Bool => (true, BuiltInType.Bool),
                TokenKind.I8 => (true, BuiltInType.I8),
                TokenKind.I16 => (true, BuiltInType.I16),
                TokenKind.I32 => (true, BuiltInType.I32),
                TokenKind.I64 => (true, BuiltInType.I64),
                TokenKind.I128 => (true, BuiltInType.I128),
                TokenKind.U8 => (true, BuiltInType.U8),
                TokenKind.U16 => (true, BuiltInType.U16),
                TokenKind.U32 => (true, BuiltInType.U32),
                TokenKind.U64 => (true, BuiltInType.U64),
                TokenKind.U128 => (true, BuiltInType.U128),
                TokenKind.ISize => (true, BuiltInType.ISize),
                TokenKind.USize => (true, BuiltInType.USize),
                TokenKind.F16 => (true, BuiltInType.F16),
                TokenKind.F32 => (true, BuiltInType.F32),
                TokenKind.F64 => (true, BuiltInType.F64),
                TokenKind.Char => (true, BuiltInType.Char),
                TokenKind.Char16 => (true, BuiltInType.Char16),
                TokenKind.Rune => (true, BuiltInType.Rune),
                TokenKind.Str => (true, BuiltInType.Str),
                _ => (false, default),
            };
            return success;
        }
    }
}
