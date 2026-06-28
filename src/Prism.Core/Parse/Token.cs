// @file Token.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using EnumFastToStringGenerated;
using Prism.Core.Source;

namespace Prism.Core.Parse;

public readonly record struct Token(
    TokenKind Kind,
    TextSpan Span,
    TokenFlags Flags = TokenFlags.None
);

[Flags]
public enum TokenFlags
{
    None,
    Synthetic = 1 << 0,
}

[EnumGenerator]
public enum TokenKind
{
    Eof,
    Unrecognized,
    UnterminatedBlockComment,
    UnterminatedStringLiteral,
    DocComment,

    [Display(Name = "'var'")]
    Var,

    [Display(Name = "'func'")]
    Func,

    [Display(Name = "'extern'")]
    Extern,

    [Display(Name = "'return'")]
    Return,

    [Display(Name = "'mut'")]
    Mut,

    [Display(Name = "'true'")]
    True,

    [Display(Name = "'false'")]
    False,

    [Display(Name = "'void'")]
    Void,

    [Display(Name = "'bool'")]
    Bool,

    [Display(Name = "'i8'")]
    I8,

    [Display(Name = "'i16'")]
    I16,

    [Display(Name = "'i32'")]
    I32,

    [Display(Name = "'i64'")]
    I64,

    [Display(Name = "'isize'")]
    // ReSharper disable once InconsistentNaming
    ISize,

    [Display(Name = "'u8'")]
    U8,

    [Display(Name = "'u16'")]
    U16,

    [Display(Name = "'u32'")]
    U32,

    [Display(Name = "'u64'")]
    U64,

    [Display(Name = "'usize'")]
    USize,

    [Display(Name = "'f32'")]
    F32,

    [Display(Name = "'f64'")]
    F64,

    [Display(Name = "'char'")]
    Char,

    [Display(Name = "'char16'")]
    Char16,

    [Display(Name = "'rune'")]
    Rune,

    [Display(Name = "'str'")]
    Str,

    [Display(Name = "[")]
    LBracket,

    [Display(Name = "]")]
    RBracket,

    [Display(Name = "(")]
    LParen,

    [Display(Name = ")")]
    RParen,

    [Display(Name = "{")]
    LBrace,

    [Display(Name = "}")]
    RBrace,

    [Display(Name = ".")]
    Period,

    [Display(Name = "...")]
    Ellipsis,

    [Display(Name = "&")]
    Amp,

    [Display(Name = "&&")]
    AmpAmp,

    [Display(Name = "&=")]
    AmpEqual,

    [Display(Name = "*")]
    Star,

    [Display(Name = "*=")]
    StarEqual,

    [Display(Name = "+")]
    Plus,

    [Display(Name = "++")]
    PlusPlus,

    [Display(Name = "+=")]
    PlusEqual,

    [Display(Name = "-")]
    Minus,

    [Display(Name = "->")]
    Arrow,

    [Display(Name = "--")]
    MinusMinus,

    [Display(Name = "-=")]
    MinusEqual,

    [Display(Name = "~")]
    Tilde,

    [Display(Name = "!")]
    Exclaim,

    [Display(Name = "!=")]
    ExclaimEqual,

    [Display(Name = "/")]
    Slash,

    [Display(Name = "/=")]
    SlashEqual,

    [Display(Name = "%")]
    Percent,

    [Display(Name = "%=")]
    PercentEqual,

    [Display(Name = "<")]
    Less,

    [Display(Name = "<<")]
    LessLess,

    [Display(Name = "<=")]
    LessEqual,

    [Display(Name = "<<=")]
    LessLessEqual,

    [Display(Name = "<=>")]
    Spaceship,

    [Display(Name = ">")]
    Greater,

    [Display(Name = ">>")]
    GreaterGreater,

    [Display(Name = ">>>")]
    GreaterGreaterGreater,

    [Display(Name = ">=")]
    GreaterEqual,

    [Display(Name = ">>=")]
    GreaterGreaterEqual,

    [Display(Name = ">>>=")]
    GreaterGreaterGreaterEqual,

    [Display(Name = "^")]
    Caret,

    [Display(Name = "^=")]
    CaretEqual,

    [Display(Name = "|")]
    Pipe,

    [Display(Name = "||")]
    PipePipe,

    [Display(Name = "|=")]
    PipeEqual,

    [Display(Name = "?")]
    Question,

    [Display(Name = "??")]
    QuestionQuestion,

    [Display(Name = "?.")]
    QuestionPeriod,

    [Display(Name = "??=")]
    QuestionQuestionEqual,

    [Display(Name = ":")]
    Colon,

    [Display(Name = "::")]
    ColonColon,

    [Display(Name = ";")]
    Semicolon,

    [Display(Name = "=")]
    Equal,

    [Display(Name = "=>")]
    BigArrow,

    [Display(Name = "==")]
    EqualEqual,

    [Display(Name = ",")]
    Comma,

    [Display(Name = "identifier")]
    Identifier,

    [Display(Name = "integer literal")]
    IntegerLiteral,

    [Display(Name = "float literal")]
    FloatLiteral,

    [Display(Name = "string literal")]
    StringLiteral,
}
