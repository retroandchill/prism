// @file BuiltInType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using NetEscapades.EnumGenerators;
using Prism.Core.Parse;

namespace Prism.Core.Syntax;

[EnumExtensions(
    ExtensionClassName = "BuiltInTypes",
    ExtensionClassNamespace = " Prism.Core.Semantic"
)]
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
        public static bool TryGet(SyntaxKind kind, out BuiltInType type)
        {
            (var success, type) = kind switch
            {
                SyntaxKind.VoidKeyword => (true, BuiltInType.Void),
                SyntaxKind.BoolKeyword => (true, BuiltInType.Bool),
                SyntaxKind.I8Keyword => (true, BuiltInType.I8),
                SyntaxKind.I16Keyword => (true, BuiltInType.I16),
                SyntaxKind.I32Keyword => (true, BuiltInType.I32),
                SyntaxKind.I64Keyword => (true, BuiltInType.I64),
                SyntaxKind.I128Keyword => (true, BuiltInType.I128),
                SyntaxKind.U8Keyword => (true, BuiltInType.U8),
                SyntaxKind.U16Keyword => (true, BuiltInType.U16),
                SyntaxKind.U32Keyword => (true, BuiltInType.U32),
                SyntaxKind.U64Keyword => (true, BuiltInType.U64),
                SyntaxKind.U128Keyword => (true, BuiltInType.U128),
                SyntaxKind.ISizeKeyword => (true, BuiltInType.ISize),
                SyntaxKind.USizeKeyword => (true, BuiltInType.USize),
                SyntaxKind.F16Keyword => (true, BuiltInType.F16),
                SyntaxKind.F32Keyword => (true, BuiltInType.F32),
                SyntaxKind.F64Keyword => (true, BuiltInType.F64),
                SyntaxKind.CharKeyword => (true, BuiltInType.Char),
                SyntaxKind.Char16Keyword => (true, BuiltInType.Char16),
                SyntaxKind.RuneKeyword => (true, BuiltInType.Rune),
                SyntaxKind.StrKeyword => (true, BuiltInType.Str),
                _ => (false, default),
            };
            return success;
        }
    }
}
