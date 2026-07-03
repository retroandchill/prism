// @file BuiltInType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
