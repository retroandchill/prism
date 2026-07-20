// @file TextCursor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Text;

public sealed class TextCursor(string text)
{
    public int Position { get; private set; }

    public bool AtEnd => Position >= text.Length;

    public char Current => text[Position];

    public ReadOnlySpan<char> Remaining => text.AsSpan(Position);

    public char Peek(int offset = 1)
    {
        var pos = Position + offset;
        return pos < text.Length ? text[pos] : '\0';
    }

    public bool Any(params ReadOnlySpan<char> options)
    {
        return options.Contains(Current);
    }

    public void Advance(int characters = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(characters);
        Position += characters;
    }

    public ReadOnlySpan<char> Since(int start)
    {
        var position = AtEnd ? text.Length : Position;
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, position);
        return text.AsSpan(start, position - start);
    }
}
