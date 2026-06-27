// @file SourceFile.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Source;

public class SourceFile
{
    public string Text { get; }
    private readonly ImmutableArray<int> _lineStarts;

    public SourceFile(string text)
    {
        Text = text;

        var builder = ImmutableArray.CreateBuilder<int>();
        foreach (var (i, current) in text.Index())
        {
            if (current == '\n')
                builder.Add(i);
        }

        _lineStarts = builder.DrainToImmutable();
    }

    public (int Line, int Column) GetLineAndColumn(int position)
    {
        var index = _lineStarts.BinarySearch(position);
        if (index < 0)
        {
            index = ~index - 1;
        }

        var lineStart = _lineStarts[index];
        var column = position - lineStart;

        return (index, column);
    }
}
