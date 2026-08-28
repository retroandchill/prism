// @file TextCursor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Utilities;

public ref struct TextCursor(ReadOnlySpan<char> text)
{
    private readonly ReadOnlySpan<char> _formatMessage = text;
    public int Position { get; private set; }

    public ReadOnlySpan<char> Remaining =>
        Position < _formatMessage.Length ? _formatMessage[Position..] : [];

    public bool IsAtEnd => Position >= _formatMessage.Length;

    public char Current => Position < _formatMessage.Length ? _formatMessage[Position] : '\0';

    public void Advance(int offset = 1)
    {
        Position += offset;
    }

    public bool TryConsume(char expected)
    {
        if (Current != expected)
            return false;

        Advance();
        return true;
    }
}
