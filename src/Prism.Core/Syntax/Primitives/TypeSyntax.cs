// @file TypeSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using EnumFastToStringGenerated;
using Prism.Core.Source;

namespace Prism.Core.Syntax.Primitives;

[EnumGenerator]
public enum BuiltInType
{
    [Display(Name = "void")]
    Void,

    [Display(Name = "bool")]
    Bool,

    [Display(Name = "i8")]
    I8,

    [Display(Name = "i16")]
    I16,

    [Display(Name = "i32")]
    I32,

    [Display(Name = "i64")]
    I64,

    [Display(Name = "isize")]
    // ReSharper disable once InconsistentNaming
    ISize,

    [Display(Name = "u8")]
    U8,

    [Display(Name = "u16")]
    U16,

    [Display(Name = "u32")]
    U32,

    [Display(Name = "u64")]
    U64,

    [Display(Name = "usize")]
    USize,

    [Display(Name = "f32")]
    F32,

    [Display(Name = "f64")]
    F64,

    [Display(Name = "char")]
    Char,

    [Display(Name = "char16")]
    Char16,

    [Display(Name = "rune")]
    Rune,

    [Display(Name = "str")]
    Str,
}

public abstract record TypeSyntax(TextSpan Span) : SyntaxNode(Span);

public sealed record BuiltInTypeSyntax(BuiltInType Type, TextSpan Span) : TypeSyntax(Span);
