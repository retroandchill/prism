// @file Lexer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Utils;

namespace Prism.Core.Parse;

public sealed class Lexer(SourceFile sourceFile)
{
    private readonly TextCursor _cursor = new(sourceFile.Text);

    public Token Next()
    {
        SkipWhitespace();

        if (_cursor.AtEnd)
            return MakeEOF();

        return MatchComment()
            ?? MatchIdentifierOrKeyword()
            ?? MatchNumber()
            ?? MatchPunctuation()
            ?? MatchStringLiteral()
            ?? MakeUnrecognized();
    }

    private Token MakeEOF()
    {
        return new Token(TokenKind.EOF, new SourceRange(_cursor.Position));
    }

    private Token MakeUnrecognized()
    {
        var token = new Token(TokenKind.Unrecognized, new SourceRange(_cursor.Position, 1));
        _cursor.Advance();
        return token;
    }

    private void SkipWhitespace()
    {
        while (!_cursor.AtEnd && char.IsWhiteSpace(_cursor.Current))
            _cursor.Advance();
    }

    private Token? MatchComment()
    {
        if (_cursor.Current != '/')
            return null;

        return _cursor.Peek() switch
        {
            '/' => HandleLineComment(),
            '*' => HandleBlockComment(),
            _ => null,
        };
    }

    private Token? HandleLineComment()
    {
        var start = _cursor.Position;
        _cursor.Advance(2);
        while (!_cursor.AtEnd && !_cursor.Any('\n', '\r'))
            _cursor.Advance();
        return new Token(TokenKind.Comment, _cursor.Since(start));
    }

    private Token? HandleBlockComment()
    {
        var start = _cursor.Position;
        _cursor.Advance(2);
        while (!_cursor.AtEnd)
        {
            if (_cursor.Current == '*' && _cursor.Peek() == '/')
            {
                _cursor.Advance(2);
                return new Token(TokenKind.Comment, _cursor.Since(start));
            }

            _cursor.Advance();
        }

        return new Token(TokenKind.Comment, _cursor.Since(start), TokenFlags.Unterminated);
    }

    private Token? MatchIdentifierOrKeyword()
    {
        var remaining = _cursor.Remaining;
        var start = _cursor.Position;

        int firstSkip;
        char firstChar;
        if (_cursor.Current == '@')
        {
            firstSkip = 2;
            firstChar = _cursor.Peek();
        }
        else
        {
            firstSkip = 1;
            firstChar = _cursor.Current;
        }

        if (!char.IsAsciiLetter(firstChar) || firstChar == '_')
            return null;

        _cursor.Advance(firstSkip);
        while (
            !_cursor.AtEnd && (char.IsAsciiLetterOrDigit(_cursor.Current) || _cursor.Current == '_')
        )
            _cursor.Advance();

        var range = _cursor.Since(start);
        var identifier = remaining[..range.Length];
        return new Token(TokenKind.MatchKeyword(identifier) ?? TokenKind.Identifier, range);
    }

    private Token? MatchNumber()
    {
        var remainder = _cursor.Remaining;
        if (remainder.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return HandleHexLiteral();
        }

        if (remainder.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            return HandleBinaryLiteral();
        }

        var start = _cursor.Position;
        var foundDigits = ConsumeDigitSequence();
        var isFloat = _cursor.Current == '.';
        if (isFloat)
        {
            _cursor.Advance();
            ConsumeDigitSequence();
        }
        else if (!foundDigits)
            return null;

        isFloat &= ConsumeFloatSuffix();
        if (isFloat)
            return new Token(TokenKind.FloatingPointLiteral, _cursor.Since(start));
        ConsumeIntegerSizeSuffix();
        return new Token(TokenKind.IntegerLiteral, _cursor.Since(start));
    }

    private Token HandleHexLiteral()
    {
        var start = _cursor.Position;
        _cursor.Advance(2);

        while (!_cursor.AtEnd && char.IsAsciiHexDigit(_cursor.Current))
            _cursor.Advance();

        var range = _cursor.Since(start);
        ConsumeIntegerSizeSuffix();
        return new Token(TokenKind.IntegerLiteral, range);
    }

    private Token HandleBinaryLiteral()
    {
        var start = _cursor.Position;
        _cursor.Advance(2);

        while (_cursor is { AtEnd: false, Current: '0' or '1' })
            _cursor.Advance();

        var range = _cursor.Since(start);
        ConsumeIntegerSizeSuffix();
        return new Token(TokenKind.IntegerLiteral, range);
    }

    private void ConsumeIntegerSizeSuffix()
    {
        if (_cursor.AtEnd)
            return;

        var remaining = _cursor.Remaining;
        if (!remaining.StartsWithAny(out _, "u", "i"))
            return;

        remaining = remaining[1..];
        if (remaining.StartsWithAny(out var prefix, "8", "16", "32", "64", "s"))
            _cursor.Advance(prefix.Length + 1);
    }

    private bool ConsumeFloatSuffix()
    {
        if (_cursor.AtEnd || _cursor.Current != 'f')
            return false;

        _cursor.Advance();
        var remainder = _cursor.Remaining;
        if (remainder.StartsWithAny(out var prefix, "16", "32", "64"))
            _cursor.Advance(prefix.Length);

        return true;
    }

    private bool ConsumeDigitSequence()
    {
        if (_cursor.AtEnd || !char.IsAsciiDigit(_cursor.Current))
            return false;

        _cursor.Advance();
        while (!_cursor.AtEnd)
        {
            var current = _cursor.Current;
            if (current == '_' && char.IsAsciiDigit(_cursor.Peek()))
            {
                _cursor.Advance(2);
            }
            else if (char.IsAsciiDigit(current))
            {
                _cursor.Advance();
            }
            else
            {
                break;
            }
        }

        return true;
    }

    private Token? MatchPunctuation()
    {
        var remainder = _cursor.Remaining;
        var start = _cursor.Position;

        if (TokenKind.MatchPunctuation(remainder) is not var (kind, length))
            return null;

        _cursor.Advance(length);
        return new Token(kind, _cursor.Since(start));
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
                    return new Token(TokenKind.StringLiteral, _cursor.Since(start));
                case '\\':
                    _cursor.Advance();
                    break;
            }

            if (_cursor.Any('\r', '\n'))
                break;
        }

        return new Token(TokenKind.StringLiteral, _cursor.Since(start), TokenFlags.Unterminated);
    }
}
