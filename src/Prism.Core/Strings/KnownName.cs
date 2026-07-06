// @file KnownName.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using NetEscapades.EnumGenerators;
using Prism.Core.Ast;

namespace Prism.Core.Strings;

[EnumExtensions(ExtensionClassName = "KnownNames")]
public enum KnownName
{
    [EnumMember(Value = "<none>")]
    None,

    [EnumMember(Value = "<error>")]
    Error,

    [EnumMember(Value = "<inferring>")]
    Inferring,

    #region Built-In Types
    [EnumMember(Value = "void")]
    Void,

    [EnumMember(Value = "bool")]
    Bool,

    [EnumMember(Value = "i8")]
    I8,

    [EnumMember(Value = "i16")]
    I16,

    [EnumMember(Value = "i32")]
    I32,

    [EnumMember(Value = "i64")]
    I64,

    [EnumMember(Value = "i128")]
    I128,

    [EnumMember(Value = "isize")]
    // ReSharper disable once InconsistentNaming
    ISize,

    [EnumMember(Value = "u8")]
    U8,

    [EnumMember(Value = "u16")]
    U16,

    [EnumMember(Value = "u32")]
    U32,

    [EnumMember(Value = "u64")]
    U64,

    [EnumMember(Value = "u128")]
    U128,

    [EnumMember(Value = "usize")]
    USize,

    [EnumMember(Value = "f16")]
    F16,

    [EnumMember(Value = "f32")]
    F32,

    [EnumMember(Value = "f64")]
    F64,

    [EnumMember(Value = "char")]
    Char,

    [EnumMember(Value = "char16")]
    Char16,

    [EnumMember(Value = "rune")]
    Rune,

    [EnumMember(Value = "str")]
    Str,
    #endregion
}

public static partial class KnownNames
{
    extension(KnownName)
    {
        public static KnownName From(BuiltInType type)
        {
            return type switch
            {
                BuiltInType.Void => KnownName.Void,
                BuiltInType.Bool => KnownName.Bool,
                BuiltInType.I8 => KnownName.I8,
                BuiltInType.I16 => KnownName.I16,
                BuiltInType.I32 => KnownName.I32,
                BuiltInType.I64 => KnownName.I64,
                BuiltInType.I128 => KnownName.I128,
                BuiltInType.ISize => KnownName.ISize,
                BuiltInType.U8 => KnownName.U8,
                BuiltInType.U16 => KnownName.U16,
                BuiltInType.U32 => KnownName.U32,
                BuiltInType.U64 => KnownName.U64,
                BuiltInType.U128 => KnownName.U128,
                BuiltInType.USize => KnownName.USize,
                BuiltInType.F16 => KnownName.F16,
                BuiltInType.F32 => KnownName.F32,
                BuiltInType.F64 => KnownName.F64,
                BuiltInType.Char => KnownName.Char,
                BuiltInType.Char16 => KnownName.Char16,
                BuiltInType.Rune => KnownName.Rune,
                BuiltInType.Str => KnownName.Str,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}