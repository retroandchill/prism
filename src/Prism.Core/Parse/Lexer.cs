// @file Lexer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;
using Prism.Core.Utils;

namespace Prism.Core.Parse;

public sealed class Lexer(SourceFile sourceFile)
{
    private readonly TextCursor _cursor = new(sourceFile.Text);
    private readonly List<GreenTrivia> _collectedTrivia = [];

    public SyntaxToken Next()
    {
        var start = _cursor.Position;
        var leadingTrivia = CollectTrivia(false);

        if (_cursor.AtEnd)
            return MakeEOF(start, leadingTrivia);

        var greenToken =
            MatchIdentifierOrKeyword(leadingTrivia)
            ?? MatchNumber(leadingTrivia)
            ?? MatchPunctuation(leadingTrivia)
            ?? MatchStringLiteral(leadingTrivia)
            ?? MakeUnrecognized(leadingTrivia);

        return new SyntaxToken(greenToken.WithLeadingTrivia(leadingTrivia), start);
    }

    private static SyntaxToken MakeEOF(int start, GreenTriviaList leadingTrivia)
    {
        return new SyntaxToken(GreenTokens.EndOfFile.WithLeadingTrivia(leadingTrivia), start);
    }

    private GreenToken MakeUnrecognized(GreenTriviaList leadingTrivia)
    {
        var token = GreenTokens.EndOfFile.WithLeadingAndTrailingTrivia(
            leadingTrivia,
            CollectTrivia()
        );
        _cursor.Advance();
        return token;
    }

    private GreenTriviaList CollectTrivia(bool stopAfterNewline = true)
    {
        _collectedTrivia.Clear();
        while (!_cursor.AtEnd)
        {
            var trivia = MatchWhiteSpace() ?? MatchNewLineTrivia() ?? MatchComment();
            if (trivia is null)
                break;

            _collectedTrivia.Add(trivia);
            if (
                stopAfterNewline
                && trivia.Kind is SyntaxKind.LineCommentTrivia or SyntaxKind.NewLineTrivia
            )
                break;
        }

        return _collectedTrivia.Count > 0
            ? new GreenTriviaList(_collectedTrivia.ToArray())
            : GreenTriviaList.Empty;
    }

    private GreenTrivia? MatchWhiteSpace()
    {
        if (_cursor.AtEnd || !char.IsWhiteSpace(_cursor.Current) || _cursor.Current is '\n' or '\r')
            return null;

        using var builder = ZString.CreateStringBuilder();
        while (
            !_cursor.AtEnd
            && char.IsWhiteSpace(_cursor.Current)
            && _cursor.Current is not '\n' and not '\r'
        )
        {
            builder.Append(_cursor.Current);
            _cursor.Advance();
        }
        return builder.Length > 0
            ? new GreenTrivia(SyntaxKind.WhitespaceTrivia, builder.ToString())
            : null;
    }

    private GreenTrivia? MatchNewLineTrivia()
    {
        var span = _cursor.Remaining;
        if (span.StartsWith("\r\n"))
        {
            _cursor.Advance(2);
            return GreenTrivia.CarriageReturnLineFeed;
        }

        if (span.StartsWith("\n"))
        {
            _cursor.Advance();
            return GreenTrivia.LineFeed;
        }

        if (span.StartsWith("\r"))
        {
            _cursor.Advance();
            return GreenTrivia.CarriageReturn;
        }

        return null;
    }

    private GreenTrivia? MatchComment()
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

    private GreenTrivia HandleLineComment()
    {
        using var builder = ZString.CreateStringBuilder();
        builder.Append("//");
        _cursor.Advance(2);
        while (!_cursor.AtEnd && !_cursor.Any('\n', '\r'))
        {
            builder.Append(_cursor.Current);
            _cursor.Advance();
        }

        var span = _cursor.Remaining;
        if (span.StartsWith("\r\n"))
        {
            builder.Append("\r\n");
            _cursor.Advance(2);
        }
        else if (span.StartsWith("\n") || span.StartsWith("\r"))
        {
            builder.Append(_cursor.Current);
            _cursor.Advance();
        }

        return new GreenTrivia(SyntaxKind.LineCommentTrivia, builder.ToString());
    }

    private GreenTrivia HandleBlockComment()
    {
        using var builder = ZString.CreateStringBuilder();
        builder.Append("/*");
        _cursor.Advance(2);
        while (!_cursor.AtEnd)
        {
            builder.Append(_cursor.Current);
            if (_cursor.Current == '*' && _cursor.Peek() == '/')
            {
                _cursor.Advance(2);
                builder.Append("*/");
                return new GreenTrivia(SyntaxKind.BlockCommentTrivia, builder.ToString());
            }

            _cursor.Advance();
        }

        return new GreenTrivia(SyntaxKind.BlockCommentTrivia, builder.ToString())
        {
            Flags = SyntaxFlags.UnterminatedLiteral,
        };
    }

    private GreenToken? MatchIdentifierOrKeyword(GreenTriviaList leadingTrivia)
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
        if (SyntaxKind.MatchKeyword(identifier) is { } matchedKeyword)
        {
            return matchedKeyword
                .GetGreenToken()
                .WithLeadingAndTrailingTrivia(leadingTrivia, CollectTrivia());
        }

        return new GreenToken(
            SyntaxKind.IdentifierToken,
            sourceFile.GetSpan(range),
            leadingTrivia,
            CollectTrivia()
        );
    }

    private GreenToken? MatchNumber(GreenTriviaList leadingTrivia)
    {
        var remainder = _cursor.Remaining;
        if (remainder.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return HandleHexLiteral(leadingTrivia);
        }

        if (remainder.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            return HandleBinaryLiteral(leadingTrivia);
        }

        var start = _cursor.Position;
        var foundDigits = ConsumeDigitSequence();
        if (foundDigits && _cursor.AtEnd)
        {
            return new GreenToken(
                SyntaxKind.IntegerLiteralToken,
                sourceFile.GetSpan(_cursor.Since(start)).ToString(),
                leadingTrivia,
                CollectTrivia()
            );
        }
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
            return new GreenToken(
                SyntaxKind.FloatingPointLiteralToken,
                sourceFile.GetSpan(_cursor.Since(start)).ToString(),
                leadingTrivia,
                CollectTrivia()
            );
        ConsumeIntegerSizeSuffix();
        return new GreenToken(
            SyntaxKind.IntegerLiteralToken,
            sourceFile.GetSpan(_cursor.Since(start)).ToString(),
            leadingTrivia,
            CollectTrivia()
        );
    }

    private GreenToken HandleHexLiteral(GreenTriviaList leadingTrivia)
    {
        var start = _cursor.Position;
        _cursor.Advance(2);

        while (!_cursor.AtEnd && char.IsAsciiHexDigit(_cursor.Current))
            _cursor.Advance();

        var range = _cursor.Since(start);
        ConsumeIntegerSizeSuffix();
        return new GreenToken(
            SyntaxKind.IntegerLiteralToken,
            sourceFile.GetSpan(range).ToString(),
            leadingTrivia,
            CollectTrivia()
        );
    }

    private GreenToken HandleBinaryLiteral(GreenTriviaList leadingTrivia)
    {
        var start = _cursor.Position;
        _cursor.Advance(2);

        while (_cursor is { AtEnd: false, Current: '0' or '1' })
            _cursor.Advance();

        var range = _cursor.Since(start);
        ConsumeIntegerSizeSuffix();
        return new GreenToken(
            SyntaxKind.IntegerLiteralToken,
            sourceFile.GetSpan(range).ToString(),
            leadingTrivia,
            CollectTrivia()
        );
    }

    private void ConsumeIntegerSizeSuffix()
    {
        if (_cursor.AtEnd)
            return;

        var remaining = _cursor.Remaining;
        if (!remaining.StartsWithAny(out _, 'u', 'i'))
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

    private GreenToken? MatchPunctuation(GreenTriviaList leadingTrivia)
    {
        var remainder = _cursor.Remaining;

        if (SyntaxKind.MatchPunctuation(remainder) is not var (kind, length))
            return null;

        _cursor.Advance(length);
        return kind.GetGreenToken().WithLeadingAndTrailingTrivia(leadingTrivia, CollectTrivia());
    }

    private GreenToken? MatchStringLiteral(GreenTriviaList leadingTrivia)
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
                    return new GreenToken(
                        SyntaxKind.StringLiteralToken,
                        sourceFile.GetSpan(_cursor.Since(start)).ToString(),
                        leadingTrivia,
                        CollectTrivia()
                    );
                case '\\':
                    _cursor.Advance();
                    break;
            }

            if (_cursor.Any('\r', '\n'))
                break;
        }

        return new GreenToken(
            SyntaxKind.StringLiteralToken,
            sourceFile.GetSpan(_cursor.Since(start)).ToString(),
            leadingTrivia,
            CollectTrivia()
        )
        {
            Flags = SyntaxFlags.UnterminatedLiteral,
        };
    }
}
