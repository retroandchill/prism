using System.Numerics;

namespace Prism.Core.Text;

public readonly record struct SourcePositionSpan
    : IEqualityOperators<SourcePositionSpan, SourcePositionSpan, bool>
{
    public SourcePosition Start { get; }
    public SourcePosition End { get; }

    public SourcePositionSpan(SourcePosition start, SourcePosition end)
    {
        if (end < start)
        {
            throw new ArgumentException(
                "End position must be greater than or equal to start position."
            );
        }
    }

    public override string ToString()
    {
        return $"({Start})-({End})";
    }
}
