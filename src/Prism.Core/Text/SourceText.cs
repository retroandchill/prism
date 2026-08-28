using System.Collections.Immutable;

namespace Prism.Core.Text;

public sealed class SourceText
{
    public string Text { get; }
    private readonly ImmutableArray<int> _lineOffsets;

    public SourceText(string text)
    {
        Text = text;
        var builder = ImmutableArray.CreateBuilder<int>();
        builder.Add(0);
        for (var i = 0; i < text.Length; i++)
        {
            var current = text[i];
            switch (current)
            {
                case '\n':
                    builder.Add(i + 1);
                    break;
                case '\r':
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        i++;
                    }

                    builder.Add(i + 1);
                    break;
            }
        }

        _lineOffsets = builder.DrainToImmutable();
    }

    public SourcePosition Positionof(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Text.Length);

        var line = _lineOffsets.BinarySearch(index);
        if (line < 0)
        {
            line = ~line;
        }
        var utfOffset = index - _lineOffsets[line - 1];
        var column = 1;
        foreach (var _ in Text.AsSpan(index, utfOffset).EnumerateRunes())
        {
            column++;
        }

        return new SourcePosition(line, column);
    }

    public ReadOnlySpan<char> Slice(TextSpan span)
    {
        return Text.AsSpan(span.Start, span.Length);
    }
}
