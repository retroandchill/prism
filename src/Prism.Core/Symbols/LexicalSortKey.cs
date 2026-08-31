using System.Diagnostics;
using System.Numerics;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

internal readonly struct LexicalSortKey
    : IEquatable<LexicalSortKey>,
        IComparable<LexicalSortKey>,
        IComparisonOperators<LexicalSortKey, LexicalSortKey, bool>
{
    public int TreeOrdinal { get; private init; }
    public int Position { get; private init; }

    public bool IsInitialized => Position != InvalidIndex;

    private const int InvalidIndex = -1;
    private const int SynthesizedIndex = int.MaxValue;

    private LexicalSortKey(int treeOrdinal, int position)
    {
        Debug.Assert(position >= 0);
        Debug.Assert(treeOrdinal >= 0);
        TreeOrdinal = treeOrdinal;
        Position = position;
    }

    public LexicalSortKey(SyntaxTree tree, int position, Compilation compilation)
        : this(compilation.GetSyntaxTreeOrdinal(tree), position) { }

    public LexicalSortKey(SyntaxReference reference, Compilation compilation)
        : this(reference.SyntaxTree, reference.SourceSpan.Start, compilation) { }

    public LexicalSortKey(SourceLocation location, Compilation compilation)
        : this(location.SyntaxTree, location.SourceSpan.Start, compilation) { }

    public static readonly LexicalSortKey NotInSource = new()
    {
        TreeOrdinal = InvalidIndex,
        Position = 0,
    };

    public static readonly LexicalSortKey NotInitialized = new()
    {
        TreeOrdinal = InvalidIndex,
        Position = InvalidIndex,
    };

    public static LexicalSortKey Synthesized(int offset)
    {
        return new LexicalSortKey()
        {
            TreeOrdinal = InvalidIndex,
            Position = SynthesizedIndex - offset,
        };
    }

    public static LexicalSortKey First(LexicalSortKey lhs, LexicalSortKey rhs)
    {
        var comparison = lhs.CompareTo(rhs);
        return comparison < 0 ? lhs : rhs;
    }

    public override bool Equals(object? obj)
    {
        return obj is LexicalSortKey other && Equals(other);
    }

    public bool Equals(LexicalSortKey other)
    {
        return TreeOrdinal == other.TreeOrdinal && Position == other.Position;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TreeOrdinal, Position);
    }

    public int CompareTo(LexicalSortKey other)
    {
        if (TreeOrdinal == other.TreeOrdinal)
            return Position.CompareTo(other.Position);

        if (TreeOrdinal < 0)
        {
            return 1;
        }

        if (other.TreeOrdinal < 0)
        {
            return -1;
        }

        var comparison = TreeOrdinal.CompareTo(other.TreeOrdinal);
        Debug.Assert(comparison != 0);
        return comparison;
    }

    public static bool operator ==(LexicalSortKey left, LexicalSortKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LexicalSortKey left, LexicalSortKey right)
    {
        return !left.Equals(right);
    }

    public static bool operator >(LexicalSortKey left, LexicalSortKey right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(LexicalSortKey left, LexicalSortKey right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static bool operator <(LexicalSortKey left, LexicalSortKey right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(LexicalSortKey left, LexicalSortKey right)
    {
        return left.CompareTo(right) <= 0;
    }
}
