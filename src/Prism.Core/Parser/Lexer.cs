using System.Collections.Immutable;
using System.Numerics;
using System.Text;
using Cysharp.Text;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;
using Prism.Core.Text;
using Prism.Core.Utils;
using IntegerSuffix = Prism.Core.Syntax.IntegerSuffix;

namespace Prism.Core.Parser;

internal sealed class Lexer(string text)
{
    private readonly TextCursor _cursor = new(text);

    public GreenToken Next()
    {
        var leadingTrivia = CollectTrivia(false);

        if (_cursor.AtEnd)
            return MakeEof(leadingTrivia);

        return MatchNumber(leadingTrivia)
            ?? MatchPunctuation(leadingTrivia)
            ?? MatchCharacterLiteral(leadingTrivia)
            ?? MatchStringLiteral(leadingTrivia)
            ?? MatchIdentifierOrKeyword(leadingTrivia)
            ?? MakeBadToken(leadingTrivia);
    }

    private GreenSyntaxList<GreenTrivia> CollectTrivia(bool stopAfterNewLine = true)
    {
        var builder = GreenSyntaxList.CreateBuilder<GreenTrivia>();
        while (!_cursor.AtEnd)
        {
            var trivia = MatchWhitespace() ?? MatchNewLine() ?? MatchComment();
            if (trivia is null)
                break;

            builder.Add(trivia);
            if (
                stopAfterNewLine
                && trivia.Kind is SyntaxKind.NewLineTrivia or SyntaxKind.LineCommentTrivia
            )
                break;
        }

        return builder.BuildAndClear();
    }

    private GreenTrivia? MatchWhitespace()
    {
        if (_cursor.AtEnd || _cursor.Any('\n', '\r') || !char.IsWhiteSpace(_cursor.Current))
            return null;

        using var builder = ZString.CreateStringBuilder();
        do
        {
            builder.Append(_cursor.Current);
            _cursor.Advance();
        } while (!_cursor.AtEnd && !_cursor.Any('\n', '\r') && char.IsWhiteSpace(_cursor.Current));

        return builder.Length != 0
            ? new GreenTrivia(SyntaxKind.WhitespaceTrivia, builder.ToString())
            : null;
    }

    private GreenTrivia? MatchNewLine()
    {
        var view = _cursor.Remaining;
        if (view.StartsWith("\r\n"))
        {
            _cursor.Advance(2);
            return GreenTrivia.CarriageReturnLineFeed;
        }

        if (view.StartsWith('\n'))
        {
            _cursor.Advance();
            return GreenTrivia.LineFeed;
        }

        if (!view.StartsWith('\r'))
            return null;

        _cursor.Advance();
        return GreenTrivia.CarriageReturn;
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

        var view = _cursor.Remaining;
        if (view.StartsWith("\r\n"))
        {
            builder.Append("\r\n");
            _cursor.Advance(2);
        }
        else if (view.StartsWith('\r') || view.StartsWith('\n'))
        {
            builder.Append(_cursor.Current);
            _cursor.Advance();
        }

        return new GreenTrivia(SyntaxKind.LineCommentTrivia, builder.ToString());
    }

    private GreenTrivia HandleBlockComment()
    {
        var start = _cursor.Position;
        using var builder = ZString.CreateStringBuilder();
        builder.Append("/*");
        _cursor.Advance(2);
        while (!_cursor.AtEnd)
        {
            builder.Append(_cursor.Current);
            if (_cursor.Remaining.StartsWith("*/"))
            {
                _cursor.Advance(2);
                builder.Append("*/");
                return new GreenTrivia(SyntaxKind.BlockCommentTrivia, builder.ToString());
            }

            _cursor.Advance();
        }

        return new GreenTrivia(SyntaxKind.BlockCommentTrivia, builder.ToString())
        {
            Diagnostics =
            [
                new SyntaxDiagnosticInfo(
                    DiagnosticInfo.UnterminatedBlockComment(),
                    _cursor.Position - start
                ),
            ],
        };
    }

    private static GreenToken MakeEof(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        return GreenToken.EOF.WithLeadingTrivia(leadingTrivia);
    }

    private GreenToken MakeBadToken(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        _cursor.Advance();
        return GreenToken.BadToken.Update(leadingTrivia, CollectTrivia());
    }

    private GreenToken? MatchNumber(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        var start = _cursor.Position;
        var remainder = _cursor.Remaining;
        var value = BigInteger.Zero;
        var isFloat = false;
        var exponent = 0;
        var @base = IntegerBase.Decimal;
        if (remainder.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!HandleHexLiteral(ref value))
            {
                // If we fail to parse just grab the leading 0 as the literal
                _cursor.Advance();
            }
            else
            {
                @base = IntegerBase.Hex;
            }
        }
        else if (remainder.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            if (!HandleBinaryLiteral(ref value))
            {
                _cursor.Advance();
            }
            else
            {
                @base = IntegerBase.Binary;
            }
        }
        else
        {
            var foundDigits = ConsumeDigitSequence(
                ref value,
                ref exponent,
                char.IsDigit,
                (c, ref v, ref _) =>
                {
                    var digit = c - '0';
                    v = v * 10 + digit;
                }
            );

            if (foundDigits && _cursor.AtEnd)
            {
                var literalText = _cursor.Since(start);
                return GreenValueToken.Create(
                    new IntegerLiteralData(literalText.ToString(), value, @base),
                    leadingTrivia,
                    CollectTrivia()
                );
            }

            if (_cursor is { AtEnd: false, Current: '.' })
            {
                _cursor.Advance();
                isFloat = true;
                if (
                    !ConsumeDigitSequence(
                        ref value,
                        ref exponent,
                        char.IsDigit,
                        (c, ref v, ref e) =>
                        {
                            var digit = c - '0';
                            v = v * 10 + digit;
                            e--;
                        }
                    )
                )
                {
                    return null;
                }
            }
            else if (!foundDigits)
            {
                return null;
            }
        }

        if (isFloat)
        {
            var floatSuffix = ConsumeFloatSuffix();
            var text = _cursor.Since(start);
            return GreenValueToken.Create(
                new FloatLiteralData(text.ToString(), value, exponent, floatSuffix),
                leadingTrivia,
                CollectTrivia()
            );
        }

        if (@base == IntegerBase.Decimal)
        {
            var floatSuffix = ConsumeFloatSuffix();
            if (floatSuffix != FloatSuffix.None)
            {
                var text = _cursor.Since(start);
                return GreenValueToken.Create(
                    new FloatLiteralData(text.ToString(), value, Suffix: floatSuffix),
                    leadingTrivia,
                    CollectTrivia()
                );
            }
        }

        var suffix = ConsumeIntegerSuffix();
        var fullText = _cursor.Since(start);
        return GreenValueToken.Create(
            new IntegerLiteralData(fullText.ToString(), value, @base, suffix),
            leadingTrivia,
            CollectTrivia()
        );
    }

    private GreenToken? MatchPunctuation(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        var kind = SyntaxKind.MatchPunctuation(_cursor);
        return kind is not null
            ? GreenToken.From(kind.Value)!.Update(leadingTrivia, CollectTrivia())
            : null;
    }

    private GreenToken? MatchCharacterLiteral(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        var diagnostics = ImmutableArray.CreateBuilder<SyntaxDiagnosticInfo>();
        CharacterEncoding encoding;
        var start = _cursor.Position;
        var remaining = _cursor.Remaining;
        int advance;
        if (remaining.StartsWith("u'"))
        {
            encoding = CharacterEncoding.Utf16;
            advance = 2;
        }
        else if (remaining.StartsWith("U'"))
        {
            encoding = CharacterEncoding.Utf32;
            advance = 2;
        }
        else if (remaining.StartsWith('\''))
        {
            encoding = CharacterEncoding.Utf8;
            advance = 1;
        }
        else
        {
            return null;
        }

        _cursor.Advance(advance);
        var terminated = false;
        var character = new Rune('\0');
        if (!_cursor.AtEnd && !_cursor.Any('\n', '\r'))
        {
            var current = _cursor.Current;

            if (current != '\\')
            {
                character = new Rune(current);
                _cursor.Advance();
            }
            else if (ParseEscapeSequence(_cursor.Remaining[1..]) is var (c, len))
            {
                character = c;
                _cursor.Advance(len + 1);
            }
            else
            {
                character = new Rune(_cursor.Peek());
                if (character != new Rune('\0'))
                {
                    diagnostics.Add(
                        new SyntaxDiagnosticInfo(
                            DiagnosticInfo.UnexpectedEscape(character.ToString()),
                            _cursor.Position - start,
                            1
                        )
                    );
                    _cursor.Advance();
                }
                else
                {
                    diagnostics.Add(
                        new SyntaxDiagnosticInfo(
                            DiagnosticInfo.UnexpectedEscape(""),
                            _cursor.Position - start,
                            1
                        )
                    );
                    _cursor.Advance(2);
                }
            }
        }

        if (_cursor is { AtEnd: false, Current: '\'' })
        {
            _cursor.Advance();
            terminated = true;
        }

        if (!terminated)
        {
            diagnostics.Add(
                new SyntaxDiagnosticInfo(
                    DiagnosticInfo.UnterminatedCharacterLiteral(),
                    _cursor.Position - start
                )
            );
        }

        return GreenValueToken.Create(
            new CharacterLiteralData(_cursor.Since(start).ToString(), character, encoding),
            leadingTrivia,
            CollectTrivia(),
            diagnostics.DrainToImmutable()
        );
    }

    private GreenToken? MatchStringLiteral(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        if (_cursor.Current != '"')
            return null;

        var start = _cursor.Position;
        using var builder = ZString.CreateStringBuilder();
        var diagnostics = ImmutableArray.CreateBuilder<SyntaxDiagnosticInfo>();
        var terminated = false;
        Span<char> runeBuffer = stackalloc char[2];
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

            if (current is '\r' or '\n')
                break;

            if (current != '\\')
            {
                builder.Append(current);
                continue;
            }

            var peeked = _cursor.Remaining[1..];
            if (ParseEscapeSequence(peeked) is var (c, length))
            {
                var chars = c.EncodeToUtf16(runeBuffer);
                builder.Append(runeBuffer[..chars]);
                _cursor.Advance(length + 1);
            }
            else
            {
                var next = _cursor.Peek();
                builder.Append('\\');
                if (next != '\0')
                {
                    builder.Append(next);
                    diagnostics.Add(
                        new SyntaxDiagnosticInfo(
                            DiagnosticInfo.UnexpectedEscape(next.ToString()),
                            _cursor.Position - start,
                            1
                        )
                    );
                }
                else
                {
                    diagnostics.Add(
                        new SyntaxDiagnosticInfo(
                            DiagnosticInfo.UnexpectedEscape(""),
                            _cursor.Position - start,
                            1
                        )
                    );
                }
            }
        }

        if (!terminated)
        {
            diagnostics.Add(
                new SyntaxDiagnosticInfo(
                    DiagnosticInfo.UnterminatedStringLiteral(),
                    _cursor.Position - start
                )
            );
        }

        var slice = _cursor.Since(start);
        return GreenValueToken.Create(
            new StringLiteralData(slice.ToString(), builder.ToString()),
            leadingTrivia,
            CollectTrivia(),
            diagnostics.DrainToImmutable()
        );
    }

    private GreenToken? MatchIdentifierOrKeyword(GreenSyntaxList<GreenTrivia> leadingTrivia)
    {
        var view = _cursor.Remaining;
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

        if (!char.IsAsciiLetter(firstChar) && firstChar != '_')
            return null;

        _cursor.Advance(firstSkip);
        while (
            !_cursor.AtEnd && (char.IsAsciiLetterOrDigit(_cursor.Current) || _cursor.Current == '_')
        )
        {
            _cursor.Advance();
        }

        var length = _cursor.Position - start;
        var identifier = view[..length];

        if (!isEscaped && SyntaxKind.MatchKeyword(identifier) is { } keyword)
        {
            return GreenToken.From(keyword)!.Update(leadingTrivia, CollectTrivia());
        }

        return GreenValueToken.Create(
            new IdentifierData(identifier.ToString()),
            leadingTrivia,
            CollectTrivia()
        );
    }

    private bool HandleHexLiteral(scoped ref BigInteger value)
    {
        if (!char.IsAsciiHexDigit(_cursor.Peek(3)))
            return false;

        var exponent = 0;
        _cursor.Advance(2);
        ConsumeDigitSequence(
            ref value,
            ref exponent,
            char.IsAsciiHexDigit,
            static (c, ref v, ref _) =>
            {
                var hexDigit = char.HexDigitValue(c);
                v = v * 16 + hexDigit;
            }
        );
        return true;
    }

    private bool HandleBinaryLiteral(scoped ref BigInteger value)
    {
        if (!char.IsBinaryDigit(_cursor.Peek(3)))
            return false;

        var exponent = 0;
        _cursor.Advance(2);
        ConsumeDigitSequence(
            ref value,
            ref exponent,
            char.IsBinaryDigit,
            static (c, ref v, ref _) =>
            {
                var digit = c == '1' ? 1 : 0;
                v = v * 2 + digit;
            }
        );
        return true;
    }

    private static readonly ImmutableArray<(string Str, IntegerSuffix Suffix)> IntegerSuffixes =
    [
        ("i8", IntegerSuffix.I8),
        ("i16", IntegerSuffix.I16),
        ("i32", IntegerSuffix.I32),
        ("i64", IntegerSuffix.I64),
        ("i128", IntegerSuffix.I128),
        ("u8", IntegerSuffix.U8),
        ("u16", IntegerSuffix.U16),
        ("u32", IntegerSuffix.U32),
        ("u64", IntegerSuffix.U64),
        ("u128", IntegerSuffix.U128),
        ("iz", IntegerSuffix.ISize),
        ("uz", IntegerSuffix.USize),
    ];

    private IntegerSuffix ConsumeIntegerSuffix()
    {
        var remaining = _cursor.Remaining;
        foreach (var (str, type) in IntegerSuffixes)
        {
            if (!remaining.StartsWith(str))
                continue;
            _cursor.Advance(str.Length);
            return type;
        }

        return IntegerSuffix.None;
    }

    private static readonly ImmutableArray<(string Str, FloatSuffix Suffix)> FloatSuffixes =
    [
        ("f32", FloatSuffix.F32),
        ("f64", FloatSuffix.F64),
    ];

    private FloatSuffix ConsumeFloatSuffix()
    {
        var remaining = _cursor.Remaining;
        foreach (var (str, type) in FloatSuffixes)
        {
            if (!remaining.StartsWith(str))
                continue;
            _cursor.Advance(str.Length);
            return type;
        }

        return FloatSuffix.None;
    }

    private delegate void HandleDigit(
        char digit,
        scoped ref BigInteger value,
        scoped ref int exponent
    );

    private bool ConsumeDigitSequence(
        scoped ref BigInteger value,
        scoped ref int exponent,
        Predicate<char> predicate,
        HandleDigit onDigit
    )
    {
        if (_cursor.AtEnd || !predicate(_cursor.Current))
            return false;

        while (!_cursor.AtEnd)
        {
            var current = _cursor.Current;
            if (current == '_')
            {
                if (LookAheadForDigits(_cursor, predicate) is { } underscores)
                {
                    _cursor.Advance(underscores);
                    current = _cursor.Current;
                }
                else
                {
                    break;
                }
            }

            if (predicate(current))
            {
                _cursor.Advance();
                onDigit(current, ref value, ref exponent);
            }
            else
            {
                break;
            }
        }

        return true;

        static int? LookAheadForDigits(TextCursor cursor, Predicate<char> pred)
        {
            var lookahead = 1;
            while (true)
            {
                var c = cursor.Peek(lookahead);
                if (c == '_')
                {
                    lookahead++;
                }
                else if (pred(c))
                {
                    return lookahead;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    private readonly record struct EscapeSequence(Rune Character, int Length);

    private static EscapeSequence? ParseEscapeSequence(scoped ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return null;

        return text[0] switch
        {
            'n' => new EscapeSequence(new Rune('\n'), 1),
            'r' => new EscapeSequence(new Rune('\r'), 1),
            't' => new EscapeSequence(new Rune('\t'), 1),
            '\\' => new EscapeSequence(new Rune('\\'), 1),
            'b' => new EscapeSequence(new Rune('\b'), 1),
            'a' => new EscapeSequence(new Rune('\a'), 1),
            'v' => new EscapeSequence(new Rune('\v'), 1),
            '\'' => new EscapeSequence(new Rune('\''), 1),
            '"' => new EscapeSequence(new Rune('"'), 1),
            '0' => new EscapeSequence(new Rune('\0'), 1),
            _ => null,
        };
    }
}
