// @file SourceFile.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Text;

public sealed class SourceFile
{
    public string Text { get; }
    private readonly ImmutableArray<int> _lineStarts;

    public SourceFile(string text)
    {
        Text = text;

        var builder = ImmutableArray.CreateBuilder<int>();
        builder.Add(0);
        for (var i = 0; i < text.Length; i++)
        {
            var current = Text[i];
            switch (current)
            {
                case '\n':
                    builder.Add(i + 1);
                    break;
                case '\r':
                {
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }
                    builder.Add(i + 1);
                    break;
                }
            }
        }
        _lineStarts = builder.ToImmutable();
    }

    public SourcePosition PositionOf(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Text.Length);

        var upper = _lineStarts.BinarySearch(index);

        if (upper < 0)
        {
            upper = ~upper;
        }
        else
        {
            upper++;
        }

        var line = upper - 1;
        var start = _lineStarts[line];
        var utfOffset = index - start;
        var span = Text.AsSpan(start, utfOffset);

        var column = 0;
        foreach (var _ in span.EnumerateRunes())
        {
            column++;
        }

        return new SourcePosition(line + 1, column + 1);
    }
}
