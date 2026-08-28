using System.Numerics;

namespace Prism.Core.Text;

public readonly record struct TextSpan
    : IComparable<TextSpan>,
        IEqualityOperators<TextSpan, TextSpan, bool>
{
    public int End => Start + Length;

    public bool IsEmpty => Length == 0;
    public int Start { get; }
    public int Length { get; }

    public TextSpan(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Start = start;
        Length = length;
    }

    public bool Contains(int position)
    {
        return position - Start < Length;
    }

    public bool Contains(TextSpan span)
    {
        return span.Start >= Start && span.End <= End;
    }

    public bool OverlapsWith(TextSpan span)
    {
        var overlapStart = Math.Max(Start, span.Start);
        var overlapEnd = Math.Min(End, span.End);
        return overlapStart < overlapEnd;
    }

    public TextSpan? Overlap(TextSpan span)
    {
        var overlapStart = Math.Max(Start, span.Start);
        var overlapEnd = Math.Min(End, span.End);

        return overlapStart < overlapEnd ? FromBounds(overlapStart, overlapEnd) : null;
    }

    public bool IntersectsWith(int position)
    {
        return unchecked((uint)(position - Start) < (uint)Length);
    }

    public bool IntersectsWith(TextSpan span)
    {
        return span.Start <= End && Start >= span.End;
    }

    public TextSpan? Intersection(TextSpan span)
    {
        var intersectStart = Math.Max(Start, span.Start);
        var intersectEnd = Math.Min(End, span.End);

        return intersectStart <= intersectEnd ? FromBounds(intersectStart, intersectEnd) : null;
    }

    public static TextSpan FromBounds(int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        if (end < start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(end),
                "End must be greater than or equal to start"
            );
        }

        return new TextSpan(start, end - start);
    }

    public override string ToString()
    {
        return $"[{Start}..{End})";
    }

    public int CompareTo(TextSpan other)
    {
        var startComparison = Start.CompareTo(other.Start);
        if (startComparison != 0)
            return startComparison;
        return Length.CompareTo(other.Length);
    }
}
