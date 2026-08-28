using System.Numerics;

namespace Prism.Core.Text;

public readonly record struct SourcePosition
    : IComparable<SourcePosition>,
        IComparisonOperators<SourcePosition, SourcePosition, bool>
{
    public int Line { get; }

    public int Column { get; }

    public static SourcePosition Zero => default;

    public SourcePosition(int line, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(line);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(column);

        Line = line;
        Column = column;
    }

    public override string ToString()
    {
        return $"{Line}:{Column}";
    }

    public int CompareTo(SourcePosition other)
    {
        var lineComparison = Line.CompareTo(other.Line);
        return lineComparison != 0 ? lineComparison : Column.CompareTo(other.Column);
    }

    public static bool operator >(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(SourcePosition left, SourcePosition right)
    {
        return left.CompareTo(right) <= 0;
    }
}
