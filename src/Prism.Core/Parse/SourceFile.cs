// @file SourceFile.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZLinq;

namespace Prism.Core.Parse;

public readonly record struct SourceRange(int Start, int Length)
{
    public int End => Start + Length;

    public bool IsEmpty => Length == 0;

    public static SourceRange Empty => new(0, 0);

    public SourceRange AsEmpty() => new(Start, 0);

    public SourceRange Concat(SourceRange other)
    {
        var start = Math.Min(Start, other.Start);
        var end = Math.Max(End, other.End);
        return new SourceRange(start, end - start);
    }
}

public readonly struct SourceFile
{
    public string Text { get; }
    private readonly ImmutableArray<int> _lineOffsets;

    public SourceFile(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        Text = text;

        var builder = ImmutableArray.CreateBuilder<int>();
        foreach (var (i, current) in Text.AsValueEnumerable().Index())
        {
            if (current == '\n')
            {
                builder.Add(i);
            }
        }
        _lineOffsets = builder.DrainToImmutable();
    }

    public (int Line, int Column) GetPosition(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Text.Length);

        var line = _lineOffsets.BinarySearch(index);
        if (line < 0)
        {
            line = ~line;
        }
        return (line, index - _lineOffsets[line]);
    }

    public ReadOnlySpan<char> GetSpan(SourceRange range) => Text.AsSpan(range.Start, range.Length);

    public ReadOnlyMemory<char> GetMemory(SourceRange range) =>
        Text.AsMemory(range.Start, range.Length);
}
