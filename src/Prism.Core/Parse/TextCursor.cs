// @file TextCursor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Source;

namespace Prism.Core.Parse;

internal struct TextCursor(string text)
{
    public string Text { get; } = text;

    public int Position { get; private set; }

    public bool AtEnd => Position >= Text.Length;

    public char Current => Text[Position];

    public ReadOnlySpan<char> Remaining => Text.AsSpan(Position);

    public TextSpan EmptySpan => new(Text, Position, 0);

    public bool Any(params ReadOnlySpan<char> chars)
    {
        return chars.Contains(Current);
    }

    public char Peek(int offset = 1)
    {
        var pos = Position + offset;
        return pos < Text.Length ? Text[pos] : '\0';
    }

    public void Advance(int characters = 1)
    {
        Position += characters;
    }

    public TextSpan FromPosition(int position)
    {
        return new TextSpan(Text, position, Position - position);
    }
}
