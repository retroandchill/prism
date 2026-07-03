// @file TokenKind.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using NetEscapades.EnumGenerators;

namespace Prism.Core.Parse;

[TokenKind]
[EnumExtensions(ExtensionClassName = "Tokens")]
public enum TokenKind : ushort
{
    [EnumMember(Value = "EOF")]
    EOF,

    [EnumMember(Value = "<unrecognized>")]
    Unrecognized,

    [EnumMember(Value = "comment")]
    Comment,

    [KeywordToken]
    Var,

    [KeywordToken]
    Func,

    [KeywordToken]
    Extern,

    [KeywordToken]
    Return,

    [KeywordToken]
    Mut,

    [KeywordToken]
    True,

    [KeywordToken]
    False,

    [KeywordToken]
    Void,

    [KeywordToken]
    Bool,

    [KeywordToken]
    I8,

    [KeywordToken]
    I16,

    [KeywordToken]
    I32,

    [KeywordToken]
    I64,

    [KeywordToken]
    I128,

    [KeywordToken]
    U8,

    [KeywordToken]
    U16,

    [KeywordToken]
    U32,

    [KeywordToken]
    U64,

    [KeywordToken]
    U128,

    [KeywordToken]
    // ReSharper disable once InconsistentNaming
    ISize,

    [KeywordToken]
    USize,

    [KeywordToken]
    F16,

    [KeywordToken]
    F32,

    [KeywordToken]
    F64,

    [KeywordToken]
    Char,

    [KeywordToken]
    Char16,

    [KeywordToken]
    Rune,

    [KeywordToken]
    Str,

    [EnumMember(Value = "[")]
    [PunctuationToken]
    LBracket,

    [EnumMember(Value = "]")]
    [PunctuationToken]
    RBracket,

    [EnumMember(Value = "(")]
    [PunctuationToken]
    LParen,

    [EnumMember(Value = ")")]
    [PunctuationToken]
    RParen,

    [EnumMember(Value = "{")]
    [PunctuationToken]
    LBrace,

    [EnumMember(Value = "}")]
    [PunctuationToken]
    RBrace,

    [EnumMember(Value = ".")]
    [PunctuationToken]
    Period,

    [EnumMember(Value = "...")]
    [PunctuationToken]
    Ellipsis,

    [EnumMember(Value = "&")]
    [PunctuationToken]
    Amp,

    [EnumMember(Value = "&&")]
    [PunctuationToken]
    AmpAmp,

    [EnumMember(Value = "&=")]
    [PunctuationToken]
    AmpEqual,

    [EnumMember(Value = "*")]
    [PunctuationToken]
    Star,

    [EnumMember(Value = "*=")]
    [PunctuationToken]
    StarEqual,

    [EnumMember(Value = "+")]
    [PunctuationToken]
    Plus,

    [EnumMember(Value = "++")]
    [PunctuationToken]
    PlusPlus,

    [EnumMember(Value = "+=")]
    [PunctuationToken]
    PlusEqual,

    [EnumMember(Value = "-")]
    [PunctuationToken]
    Minus,

    [EnumMember(Value = "->")]
    [PunctuationToken]
    Arrow,

    [EnumMember(Value = "--")]
    [PunctuationToken]
    MinusMinus,

    [EnumMember(Value = "-=")]
    [PunctuationToken]
    MinusEqual,

    [EnumMember(Value = "~")]
    [PunctuationToken]
    Tilde,

    [EnumMember(Value = "!")]
    [PunctuationToken]
    Exclaim,

    [EnumMember(Value = "!=")]
    [PunctuationToken]
    ExclaimEqual,

    [EnumMember(Value = "/")]
    [PunctuationToken]
    Slash,

    [EnumMember(Value = "/=")]
    [PunctuationToken]
    SlashEqual,

    [EnumMember(Value = "%")]
    [PunctuationToken]
    Percent,

    [EnumMember(Value = "%=")]
    [PunctuationToken]
    PercentEqual,

    [EnumMember(Value = "<")]
    [PunctuationToken]
    Less,

    [EnumMember(Value = "<<")]
    [PunctuationToken]
    LessLess,

    [EnumMember(Value = "<=")]
    [PunctuationToken]
    LessEqual,

    [EnumMember(Value = "<<=")]
    [PunctuationToken]
    LessLessEqual,

    [EnumMember(Value = "<=>")]
    [PunctuationToken]
    Spaceship,

    [EnumMember(Value = ">")]
    [PunctuationToken]
    Greater,

    [EnumMember(Value = ">>")]
    [PunctuationToken]
    GreaterGreater,

    [EnumMember(Value = ">>>")]
    [PunctuationToken]
    GreaterGreaterGreater,

    [EnumMember(Value = ">=")]
    [PunctuationToken]
    GreaterEqual,

    [EnumMember(Value = ">>=")]
    [PunctuationToken]
    GreaterGreaterEqual,

    [EnumMember(Value = ">>>=")]
    [PunctuationToken]
    GreaterGreaterGreaterEqual,

    [EnumMember(Value = "^")]
    [PunctuationToken]
    Caret,

    [EnumMember(Value = "^=")]
    [PunctuationToken]
    CaretEqual,

    [EnumMember(Value = "|")]
    [PunctuationToken]
    Pipe,

    [EnumMember(Value = "||")]
    [PunctuationToken]
    PipePipe,

    [EnumMember(Value = "|=")]
    [PunctuationToken]
    PipeEqual,

    [EnumMember(Value = "?")]
    [PunctuationToken]
    Question,

    [EnumMember(Value = "??")]
    [PunctuationToken]
    QuestionQuestion,

    [EnumMember(Value = "?.")]
    [PunctuationToken]
    QuestionPeriod,

    [EnumMember(Value = ":")]
    [PunctuationToken]
    Colon,

    [EnumMember(Value = "::")]
    [PunctuationToken]
    ColonColon,

    [EnumMember(Value = ";")]
    [PunctuationToken]
    Semicolon,

    [EnumMember(Value = "=")]
    [PunctuationToken]
    Equal,

    [EnumMember(Value = "=>")]
    [PunctuationToken]
    BigArrow,

    [EnumMember(Value = "==")]
    [PunctuationToken]
    EqualEqual,

    [EnumMember(Value = ",")]
    [PunctuationToken]
    Comma,

    [EnumMember(Value = "identifier")]
    Identifier,

    [EnumMember(Value = "integer literal")]
    IntegerLiteral,

    [EnumMember(Value = "floating point literal")]
    FloatingPointLiteral,

    [EnumMember(Value = "string literal")]
    StringLiteral,
}
