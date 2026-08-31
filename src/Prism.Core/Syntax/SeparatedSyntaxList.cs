using System.Collections;
using System.Diagnostics;
using Prism.Core.Syntax.Green;
using ZLinq;

namespace Prism.Core.Syntax;

public readonly struct SeparatedSyntaxList<T>
    : IReadOnlyList<T>,
        IValueEnumerable<SeparatedSyntaxList<T>.Enumerator, T>
    where T : SyntaxNode
{
    private readonly SyntaxNodeOrTokenList _list;

    internal SeparatedSyntaxList(SyntaxNodeOrTokenList list)
    {
        Validate(list);
        _list = list;
    }

    internal SeparatedSyntaxList(SyntaxNode? node)
        : this(new SyntaxNodeOrTokenList(node)) { }

    [Conditional("DEBUG")]
    private static void Validate(SyntaxNodeOrTokenList list)
    {
        foreach (var (i, item) in list.AsValueEnumerable().Index())
        {
            Debug.Assert(i % 2 == 0 ? item.IsNode : item.IsToken);
        }
    }

    public int Count => (_list.Count + 1) / 2;

    public int SeparatorCount => _list.Count / 2;

    public T this[int index]
    {
        get
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index must be non-negative");

            var node = _list.Node;
            if (node is null)
                throw new IndexOutOfRangeException("List is empty");

            if (!node.Green.IsList)
            {
                if (index != 0)
                    throw new IndexOutOfRangeException(
                        "Index must be 0 for non-list separated syntax lists"
                    );

                return (T)node;
            }

            if (index >= Count)
                throw new IndexOutOfRangeException("Index out of range");

            return (T)node.GetRequiredNodeSlot(index * 2);
        }
    }

    public SyntaxToken GetSeparator(int index)
    {
        if (index < 0)
            throw new IndexOutOfRangeException("Index must be non-negative");

        var node = _list.Node;
        if (node is null)
            throw new IndexOutOfRangeException("List is empty");

        Debug.Assert(node.Green.IsList, "separator cannot appear in a non-list");
        if (index >= SeparatorCount)
            throw new IndexOutOfRangeException("Index out of range");

        var green = node.Green.GetRequiredSlot<GreenToken>(index * 2 + 1);
        Debug.Assert(green is not null);
        return new SyntaxToken(green, node, node.GetSlotPosition(index * 2 + 1));
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, T> AsValueEnumerable() => new(GetEnumerator());

    public struct Enumerator : IEnumerator<T>, IValueEnumerator<T>
    {
        private readonly SeparatedSyntaxList<T> _list;
        private readonly int _count;
        private int _index;

        public T Current
        {
            get => field ?? throw new InvalidOperationException("Enumerator not started");
            private set;
        }
        object IEnumerator.Current => Current;

        public Enumerator(SeparatedSyntaxList<T> list)
        {
            _list = list;
            _count = list.Count;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (_index >= _count)
                return false;

            _index++;
            Current = _list[_index];
            return true;
        }

        public bool TryGetNext(out T current)
        {
            if (!MoveNext())
            {
                current = null!;
                return false;
            }

            current = Current;
            return true;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
