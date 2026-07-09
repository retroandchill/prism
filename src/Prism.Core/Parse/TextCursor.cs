// @file TextCursor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Parse;

public sealed class TextCursor
{
    private readonly string _text;

    public int Position { get; private set; }

    public bool AtEnd => Position >= _text.Length;

    public char Current => _text[Position];

    public ReadOnlySpan<char> Remaining => _text.AsSpan(Position);

    public TextCursor(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
    }

    public char Peek(int offset = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 1);

        var pos = Position + offset;
        return pos < _text.Length ? _text[pos] : '\0';
    }

    public bool Any(params ReadOnlySpan<char> options)
    {
        return options.Contains(Current);
    }

    public void Advance(int characters = 1)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(characters, 1);
        Position += characters;
    }

    public ReadOnlySpan<char> Since(int start)
    {
        var position = AtEnd ? _text.Length : Position;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, position);
        return _text.AsSpan(start, position - start);
    }
}
