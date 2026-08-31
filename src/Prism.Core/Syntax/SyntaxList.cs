using System.Collections;
using Prism.Core.Syntax.Green;
using ZLinq;

namespace Prism.Core.Syntax;

internal sealed class SyntaxList : SyntaxNode
{
    private readonly SyntaxNode?[] _children;

    internal int Count => _children.Length;

    public SyntaxList(GreenListNode node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
        _children = new SyntaxNode?[node.SlotCount];
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _children[index]) : GetRed(ref _children[index], index);
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return _children[index];
    }
}

public readonly struct SyntaxList<T>
    : IReadOnlyList<T>,
        IValueEnumerable<SyntaxList<T>.Enumerator, T>
    where T : SyntaxNode
{
    private readonly SyntaxNode? _node;

    internal SyntaxList(SyntaxNode? node)
    {
        _node = node;
    }

    public int Count
    {
        get
        {
            if (_node is null)
                return 0;

            return _node.Green.IsList ? _node.Green.SlotCount : 1;
        }
    }

    public T this[int index]
    {
        get
        {
            if (index < 0)
                throw new IndexOutOfRangeException("Index must be non-negative");

            switch (_node)
            {
                case null:
                case { Green.IsList: true } when index >= _node.Green.SlotCount:
                    throw new IndexOutOfRangeException();
                case { Green.IsList: true }:
                    return (T)_node.GetRequiredNodeSlot(index);
            }

            if (index != 0)
                throw new IndexOutOfRangeException();

            return (T)_node;
        }
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, T> AsValueEnumerable() => new(GetEnumerator());

    public struct Enumerator : IEnumerator<T>, IValueEnumerator<T>
    {
        private readonly SyntaxList<T> _list;
        private readonly int _count;
        private int _index;

        public T Current
        {
            get => field ?? throw new InvalidOperationException("Enumerator not started");
            private set;
        }
        object IEnumerator.Current => Current;

        public Enumerator(SyntaxList<T> list)
        {
            _list = list;
            _count = list.Count;
            _index = -1;
        }

        public bool MoveNext()
        {
            if (_index + 1 >= _count)
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
