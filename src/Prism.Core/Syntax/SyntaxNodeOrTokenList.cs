using System.Collections;
using Prism.Core.Syntax.Green;
using ZLinq;

namespace Prism.Core.Syntax;

public readonly struct SyntaxNodeOrTokenList
    : IReadOnlyList<SyntaxNodeOrToken>,
        IValueEnumerable<SyntaxNodeOrTokenList.Enumerator, SyntaxNodeOrToken>
{
    internal SyntaxNode? Node { get; }

    internal SyntaxNodeOrTokenList(SyntaxNode? node)
    {
        Node = node;
    }

    public int Count
    {
        get
        {
            if (Node is null)
                return 0;

            return Node.Green.IsList ? Node.Green.SlotCount : 1;
        }
    }

    public SyntaxNodeOrToken this[int index]
    {
        get
        {
            if (Node is null)
                throw new IndexOutOfRangeException();

            if (!Node.Green.IsList)
            {
                if (index > 0)
                    throw new IndexOutOfRangeException();

                return Node;
            }

            if (index >= Node.Green.SlotCount)
                throw new IndexOutOfRangeException();

            var token = Node.Green.GetSlot<GreenToken>(index);
            if (token is not null)
            {
                return new SyntaxToken(token, Parent, Node.GetSlotPosition(index));
            }

            return Node.GetRequiredNodeSlot(index);
        }
    }

    private SyntaxNode? Parent => Node?.Parent;

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator() =>
        GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, SyntaxNodeOrToken> AsValueEnumerable() =>
        new(GetEnumerator());

    public struct Enumerator : IEnumerator<SyntaxNodeOrToken>, IValueEnumerator<SyntaxNodeOrToken>
    {
        private readonly SyntaxNodeOrTokenList _list;
        private readonly int _count;
        private int _index;

        public SyntaxNodeOrToken Current { get; private set; }
        object IEnumerator.Current => Current;

        public Enumerator(SyntaxNodeOrTokenList list)
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

        public bool TryGetNext(out SyntaxNodeOrToken current)
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

        public bool TryGetSpan(out ReadOnlySpan<SyntaxNodeOrToken> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<SyntaxNodeOrToken> destination, Index offset)
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
