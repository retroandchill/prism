// @file TokenKind.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using NetEscapades.EnumGenerators;
using Prism.Core.Parse;

namespace Prism.Core.Syntax;

[TokenKind]
[EnumExtensions(ExtensionClassName = "SyntaxKinds")]
public enum SyntaxKind : ushort
{
    None = 0,

    #region Trivia

    WhitespaceTrivia = 1,
    NewLineTrivia,
    LineCommentTrivia,
    BlockCommentTrivia,
    SkippedTextTrivia,

    #endregion

    #region Tokens

    EndOfFileToken = 100,
    BadToken,

    [KeywordToken("var")]
    VarKeyword,

    [KeywordToken("func")]
    FuncKeyword,

    [KeywordToken("extern")]
    ExternKeyword,

    [KeywordToken("return")]
    ReturnKeyword,

    [KeywordToken("mut")]
    MutKeyword,

    [KeywordToken("true")]
    TrueKeyword,

    [KeywordToken("false")]
    FalseKeyword,

    [KeywordToken("void")]
    VoidKeyword,

    [KeywordToken("bool")]
    BoolKeyword,

    [KeywordToken("i8")]
    I8Keyword,

    [KeywordToken("i16")]
    I16Keyword,

    [KeywordToken("i32")]
    I32Keyword,

    [KeywordToken("i64")]
    I64Keyword,

    [KeywordToken("i128")]
    I128Keyword,

    [KeywordToken("u8")]
    U8Keyword,

    [KeywordToken("u16")]
    U16Keyword,

    [KeywordToken("u32")]
    U32Keyword,

    [KeywordToken("u64")]
    U64Keyword,

    [KeywordToken("u128")]
    U128Keyword,

    [KeywordToken("isize")]
    ISizeKeyword,

    [KeywordToken("usize")]
    USizeKeyword,

    [KeywordToken("f16")]
    F16Keyword,

    [KeywordToken("f32")]
    F32Keyword,

    [KeywordToken("f64")]
    F64Keyword,

    [KeywordToken("char")]
    CharKeyword,

    [KeywordToken("char16")]
    Char16Keyword,

    [KeywordToken("rune")]
    RuneKeyword,

    [KeywordToken("str")]
    StrKeyword,

    [PunctuationToken("[")]
    LBracketToken,

    [PunctuationToken("]")]
    RBracketToken,

    [PunctuationToken("(")]
    LParenToken,

    [PunctuationToken(")")]
    RParenToken,

    [PunctuationToken("{")]
    LBraceToken,

    [PunctuationToken("}")]
    RBraceToken,

    [PunctuationToken(".")]
    PeriodToken,

    [PunctuationToken("...")]
    EllipsisToken,

    [PunctuationToken("&")]
    AmpToken,

    [PunctuationToken("&&")]
    AmpAmpToken,

    [PunctuationToken("&=")]
    AmpEqualToken,

    [PunctuationToken("*")]
    StarToken,

    [PunctuationToken("*=")]
    StarEqualToken,

    [PunctuationToken("+")]
    PlusToken,

    [PunctuationToken("++")]
    PlusPlusToken,

    [PunctuationToken("+=")]
    PlusEqualToken,

    [PunctuationToken("-")]
    MinusToken,

    [PunctuationToken("->")]
    ArrowToken,

    [PunctuationToken("--")]
    MinusMinusToken,

    [PunctuationToken("-=")]
    MinusEqualToken,

    [PunctuationToken("~")]
    TildeToken,

    [PunctuationToken("!")]
    ExclaimToken,

    [PunctuationToken("!=")]
    ExclaimEqualToken,

    [PunctuationToken("/")]
    SlashToken,

    [PunctuationToken("/=")]
    SlashEqualToken,

    [PunctuationToken("%")]
    PercentToken,

    [PunctuationToken("%=")]
    PercentEqualToken,

    [PunctuationToken("<")]
    LessToken,

    [PunctuationToken("<<")]
    LessLessToken,

    [PunctuationToken("<=")]
    LessEqualToken,

    [PunctuationToken("<<=")]
    LessLessEqualToken,

    [PunctuationToken("<=>")]
    SpaceshipToken,

    [PunctuationToken(">")]
    GreaterToken,

    [PunctuationToken(">>")]
    GreaterGreaterToken,

    [PunctuationToken(">>>")]
    GreaterGreaterGreaterToken,

    [PunctuationToken(">=")]
    GreaterEqualToken,

    [PunctuationToken(">>=")]
    GreaterGreaterEqualToken,

    [PunctuationToken(">>>=")]
    GreaterGreaterGreaterEqualToken,

    [PunctuationToken("^")]
    CaretToken,

    [PunctuationToken("^=")]
    CaretEqualToken,

    [PunctuationToken("|")]
    PipeToken,

    [PunctuationToken("||")]
    PipePipeToken,

    [PunctuationToken("|=")]
    PipeEqualToken,

    [PunctuationToken("?")]
    QuestionToken,

    [PunctuationToken("??")]
    QuestionQuestionToken,

    [PunctuationToken("?.")]
    QuestionPeriodToken,

    [PunctuationToken(":")]
    ColonToken,

    [PunctuationToken("::")]
    ColonColonToken,

    [PunctuationToken(";")]
    SemicolonToken,

    [PunctuationToken("=")]
    EqualToken,

    [PunctuationToken("=>")]
    BigArrowToken,

    [PunctuationToken("==")]
    EqualEqualToken,

    [PunctuationToken(",")]
    CommaToken,

    IdentifierToken,

    IntegerLiteralToken,

    FloatingPointLiteralToken,

    CharacterLiteralToken,

    StringLiteralToken,

    #endregion

    #region Syntax

    TriviaList = 1000,

    #endregion
}
