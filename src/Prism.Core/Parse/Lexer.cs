// @file Lexer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Cysharp.Text;
using Prism.Core.Diagnostics;
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
            Diagnostics = [ParseDiagnostics.UnterminatedBlockComment()],
        };
    }

    private GreenToken? MatchIdentifierOrKeyword(GreenTriviaList leadingTrivia)
    {
        var remaining = _cursor.Remaining;
        var start = _cursor.Position;

        int firstSkip;
        char firstChar;
        bool isEscaped;
        if (_cursor.Current == '@')
        {
            firstSkip = 2;
            firstChar = _cursor.Peek();
            isEscaped = true;
        }
        else
        {
            firstSkip = 1;
            firstChar = _cursor.Current;
            isEscaped = false;
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
            var baseToken = matchedKeyword switch
            {
                SyntaxKind.TrueKeyword => GreenLiteralToken.BoolTrue,
                SyntaxKind.FalseKeyword => GreenLiteralToken.BoolFalse,
                _ => matchedKeyword.GetGreenToken(),
            };

            return baseToken.WithLeadingAndTrailingTrivia(leadingTrivia, CollectTrivia());
        }

        var info = new IdentifierInfo(isEscaped ? identifier[1..] : identifier, isEscaped);
        return new GreenIdentifierToken(
            SyntaxKind.IdentifierToken,
            info,
            leadingTrivia,
            CollectTrivia()
        );
    }

    private GreenToken? MatchNumber(GreenTriviaList leadingTrivia)
    {
        var start = _cursor.Position;
        var remainder = _cursor.Remaining;
        bool isFloat;
        if (remainder.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            isFloat = false;
            if (!HandleHexLiteral())
            {
                // If we fail to parse just grab the leading 0 as the literal
                _cursor.Advance();
            }
        }
        else if (remainder.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            isFloat = false;
            if (!HandleBinaryLiteral())
            {
                // If we fail to parse just grab the leading 0 as the literal
                _cursor.Advance();
            }
        }
        else
        {
            var foundDigits = ConsumeDigitSequence(char.IsAsciiDigit);
            if (foundDigits && _cursor.AtEnd)
            {
                var literalText = _cursor.Since(start);
                var info = Numerics.ParseInteger(literalText);
                return GreenLiteralToken.Create(
                    info,
                    literalText.ToString(),
                    leadingTrivia,
                    CollectTrivia()
                );
            }
            isFloat = _cursor.Current == '.';
            if (isFloat)
            {
                _cursor.Advance();
                ConsumeDigitSequence(char.IsAsciiDigit);
            }
            else if (!foundDigits)
                return null;

            isFloat &= ConsumeFloatSuffix();
            if (!isFloat)
            {
                ConsumeIntegerSizeSuffix();
            }
        }

        var text = _cursor.Since(start);
        if (isFloat)
        {
            var info = Numerics.ParseFloat(text);
            return GreenLiteralToken.Create(info, text.ToString(), leadingTrivia, CollectTrivia());
        }
        else
        {
            var info = Numerics.ParseInteger(text);
            return GreenLiteralToken.Create(info, text.ToString(), leadingTrivia, CollectTrivia());
        }
    }

    private bool HandleHexLiteral()
    {
        if (!char.IsAsciiHexDigit(_cursor.Peek(3)))
            return false;

        _cursor.Advance(2);

        ConsumeDigitSequence(char.IsAsciiHexDigit);

        ConsumeIntegerSizeSuffix();
        return true;
    }

    private bool HandleBinaryLiteral()
    {
        if (_cursor.Peek(3) is not ('0' or '1'))
            return false;

        _cursor.Advance(2);

        ConsumeDigitSequence(c => c is '0' or '1');

        ConsumeIntegerSizeSuffix();
        return true;
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

    private bool ConsumeDigitSequence(Func<char, bool> predicate)
    {
        if (_cursor.AtEnd || !predicate(_cursor.Current))
            return false;

        _cursor.Advance();
        while (!_cursor.AtEnd)
        {
            var current = _cursor.Current;
            if (current == '_' && predicate(_cursor.Peek()))
            {
                _cursor.Advance(2);
            }
            else if (predicate(current))
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
        using var builder = ZString.CreateStringBuilder();
        var diagnostics = ImmutableArray.CreateBuilder<DiagnosticInfo>();
        var terminated = false;
        while (!_cursor.AtEnd)
        {
            _cursor.Advance();

            var current = _cursor.Current;
            if (current == '"')
            {
                _cursor.Advance();
                terminated = true;
                break;
            }

            if (_cursor.Any('\r', '\n'))
                break;

            if (current != '\\')
            {
                builder.Append(current);
                continue;
            }

            var next = _cursor.Peek();
            switch (next)
            {
                case 'n':
                    builder.Append('\n');
                    break;
                case 'r':
                    builder.Append('\r');
                    break;
                case 't':
                    builder.Append('\t');
                    break;
                case '\\':
                    builder.Append(@"\\");
                    break;
                case 'b':
                    builder.Append(@"\b");
                    break;
                case '\'':
                    builder.Append(@"\'");
                    break;
                case '"':
                    builder.Append('"');
                    break;
                default:
                    builder.Append('\\');
                    builder.Append(next);
                    diagnostics.Add(ParseDiagnostics.UnexpectedEscape($@"\{next}"));
                    break;
            }

            _cursor.Advance(2);
        }

        if (!terminated)
        {
            diagnostics.Add(ParseDiagnostics.UnterminatedStringLiteral());
        }

        var literal = new StringLiteralValue(builder.ToString(), CharacterEncoding.Utf8);
        return new GreenLiteralToken<StringLiteralValue>(
            literal,
            _cursor.Since(start).ToString(),
            leadingTrivia,
            CollectTrivia()
        )
        {
            Diagnostics = diagnostics.DrainToImmutable(),
        };
    }
}
