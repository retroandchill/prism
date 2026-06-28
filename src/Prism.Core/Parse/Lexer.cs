// @file Lexer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZLinq;

namespace Prism.Core.Parse;

public sealed class Lexer(string text)
{
    private TextCursor _cursor = new(text);

    public Token NextToken()
    {
        SkipWhitespace();

        if (_cursor.AtEnd)
            return MakeEof();

        if (MatchComment() is { } comment)
        {
            return comment;
        }

        if (_cursor.AtEnd)
            return MakeEof();

        return MatchIdentifierOrKeyword()
            ?? MatchNumber()
            ?? MatchOperator()
            ?? MatchStringLiteral()
            ?? MakeUnrecognized();
    }

    private Token MakeEof()
    {
        return new Token(TokenKind.Eof, _cursor.EmptySpan);
    }

    private void SkipWhitespace()
    {
        while (!_cursor.AtEnd && char.IsWhiteSpace(_cursor.Current))
        {
            _cursor.Advance();
        }
    }

    private Token? MatchComment()
    {
        if (_cursor.Current != '/')
            return null;

        if (_cursor.Peek() == '/')
        {
            SkipLineComment();
        }
        else if (_cursor.Peek() == '*')
        {
            return SkipBlockComment();
        }
        SkipWhitespace();

        return null;
    }

    private void SkipLineComment()
    {
        while (_cursor is { AtEnd: false, Current: not '\n' and not '\r' })
        {
            _cursor.Advance();
        }
    }

    private Token? SkipBlockComment()
    {
        var start = _cursor.Position;
        _cursor.Advance(2);

        var isDocComment = false;
        if (_cursor is { AtEnd: false, Current: '*' })
        {
            isDocComment = true;
            _cursor.Advance();
        }

        while (!_cursor.AtEnd)
        {
            if (_cursor.Current == '*' && _cursor.Peek() == '/')
            {
                _cursor.Advance(2);

                Token? comment = isDocComment
                    ? new Token(TokenKind.DocComment, _cursor.FromPosition(start))
                    : null;
                SkipWhitespace();
                return comment;
            }

            _cursor.Advance();
        }

        return new Token(TokenKind.UnterminatedBlockComment, _cursor.FromPosition(start));
    }

    private Token? MatchIdentifierOrKeyword()
    {
        var start = _cursor.Position;
        var current = _cursor.Current;
        if (!char.IsLetter(current) && current != '_')
            return null;

        while (!_cursor.AtEnd && (char.IsLetterOrDigit(_cursor.Current) || _cursor.Current == '_'))
        {
            _cursor.Advance();
        }

        var identifier = _cursor.FromPosition(start);
        var lookup = Keywords.GetAlternateLookup<ReadOnlySpan<char>>();
        return lookup.TryGetValue(identifier, out var kind)
            ? new Token(kind, identifier)
            : new Token(TokenKind.Identifier, identifier);
    }

    private Token? MatchNumber()
    {
        var start = _cursor.Position;
        if (!char.IsDigit(_cursor.Current))
            return null;

        _cursor.Advance();
        while (!_cursor.AtEnd && char.IsDigit(_cursor.Current))
        {
            _cursor.Advance();
        }

        return new Token(TokenKind.IntegerLiteral, _cursor.FromPosition(start));
    }

    private Token? MatchOperator()
    {
        var remaining = _cursor.Remaining;
        foreach (var (text, kind) in Operators)
        {
            if (!remaining.StartsWith(text))
                continue;
            _cursor.Advance(text.Length);
            return new Token(kind, _cursor.FromPosition(_cursor.Position - text.Length));
        }

        return null;
    }

    private Token? MatchStringLiteral()
    {
        if (_cursor.Current != '"')
            return null;

        var start = _cursor.Position;
        while (!_cursor.AtEnd)
        {
            _cursor.Advance();

            switch (_cursor.Current)
            {
                case '"':
                    _cursor.Advance();
                    return new Token(TokenKind.StringLiteral, _cursor.FromPosition(start));
                case '\\':
                    _cursor.Advance();
                    break;
            }

            if (_cursor.Current is '\n' or '\r')
            {
                return new Token(TokenKind.StringLiteral, _cursor.FromPosition(start));
            }
        }

        return new Token(TokenKind.StringLiteral, _cursor.FromPosition(start));
    }

    private Token MakeUnrecognized()
    {
        var position = _cursor.Position;
        _cursor.Advance();
        return new Token(TokenKind.Unrecognized, _cursor.FromPosition(position));
    }

    private static readonly Dictionary<string, TokenKind> Keywords = new()
    {
        ["var"] = TokenKind.Var,
        ["func"] = TokenKind.Func,
        ["extern"] = TokenKind.Extern,
        ["return"] = TokenKind.Return,
        ["mut"] = TokenKind.Mut,
        ["true"] = TokenKind.True,
        ["false"] = TokenKind.False,
        ["void"] = TokenKind.Void,
        ["bool"] = TokenKind.Bool,
        ["i8"] = TokenKind.I8,
        ["i16"] = TokenKind.I16,
        ["i32"] = TokenKind.I32,
        ["i64"] = TokenKind.I64,
        ["isize"] = TokenKind.ISize,
        ["u8"] = TokenKind.U8,
        ["u16"] = TokenKind.U16,
        ["u32"] = TokenKind.U32,
        ["u64"] = TokenKind.U64,
        ["usize"] = TokenKind.USize,
        ["f32"] = TokenKind.F32,
        ["f64"] = TokenKind.F64,
        ["char"] = TokenKind.Char,
        ["char16"] = TokenKind.Char16,
        ["rune"] = TokenKind.Rune,
        ["str"] = TokenKind.Str,
    };

    private static readonly ImmutableArray<(string Text, TokenKind Kind)> Operators = SortByLength(
        ("[", TokenKind.LBracket),
        ("]", TokenKind.RBracket),
        ("(", TokenKind.LParen),
        (")", TokenKind.RParen),
        ("{", TokenKind.LBrace),
        ("}", TokenKind.RBrace),
        (".", TokenKind.Period),
        ("...", TokenKind.Ellipsis),
        ("&", TokenKind.Amp),
        ("&&", TokenKind.AmpAmp),
        ("&=", TokenKind.AmpEqual),
        ("*", TokenKind.Star),
        ("*=", TokenKind.StarEqual),
        ("+", TokenKind.Plus),
        ("++", TokenKind.PlusPlus),
        ("+=", TokenKind.PlusEqual),
        ("-", TokenKind.Minus),
        ("->", TokenKind.Arrow),
        ("--", TokenKind.MinusMinus),
        ("-=", TokenKind.MinusEqual),
        ("~", TokenKind.Tilde),
        ("!", TokenKind.Exclaim),
        ("!=", TokenKind.ExclaimEqual),
        ("/", TokenKind.Slash),
        ("/=", TokenKind.SlashEqual),
        ("%", TokenKind.Percent),
        ("%=", TokenKind.PercentEqual),
        ("<", TokenKind.Less),
        ("<<", TokenKind.LessLess),
        ("<=", TokenKind.LessEqual),
        ("<<=", TokenKind.LessLessEqual),
        ("<=>", TokenKind.Spaceship),
        (">", TokenKind.Greater),
        (">>", TokenKind.GreaterGreater),
        (">>>", TokenKind.GreaterGreaterGreater),
        (">=", TokenKind.GreaterEqual),
        (">>=", TokenKind.GreaterGreaterEqual),
        (">>>=", TokenKind.GreaterGreaterGreaterEqual),
        ("^", TokenKind.Caret),
        ("^=", TokenKind.CaretEqual),
        ("|", TokenKind.Pipe),
        ("||", TokenKind.PipePipe),
        ("|=", TokenKind.PipeEqual),
        ("?", TokenKind.Question),
        ("??", TokenKind.QuestionQuestion),
        ("?.", TokenKind.QuestionPeriod),
        (":", TokenKind.Colon),
        ("::", TokenKind.ColonColon),
        (";", TokenKind.Semicolon),
        ("=", TokenKind.Equal),
        ("=>", TokenKind.BigArrow),
        ("==", TokenKind.EqualEqual),
        (",", TokenKind.Comma)
    );

    private static ImmutableArray<(string Text, TokenKind Kind)> SortByLength(
        params ReadOnlySpan<(string Text, TokenKind Kind)> operators
    )
    {
        return operators
            .AsValueEnumerable()
            .OrderByDescending(x => x.Text.Length)
            .ToImmutableArray();
    }
}
