using System.Collections;
using Prism.Core.Syntax.Green;
using ZLinq;

namespace Prism.Core.Syntax;

public readonly struct SyntaxTriviaList
    : IReadOnlyList<SyntaxTrivia>,
        IValueEnumerable<SyntaxTriviaList.Enumerator, SyntaxTrivia>
{
    public static SyntaxTriviaList Empty => new(default, null);

    private readonly SyntaxToken _parent;
    private readonly GreenSyntaxList<GreenNode> _green;
    private readonly int _position;

    public int Count => _green.Count;

    public SyntaxTrivia this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            var element = _green[index];
            var position = _parent.Position + _green.Node?.GetSlotOffset(index) ?? 0;
            return new SyntaxTrivia(in _parent, element, position);
        }
    }

    internal SyntaxTriviaList(in SyntaxToken parent, GreenNode? green)
    {
        _parent = parent;
        _green = new GreenSyntaxList<GreenNode>(green);
        _position = parent.Position;
    }

    internal SyntaxTriviaList(in SyntaxToken parent, GreenNode? green, int position)
    {
        _parent = parent;
        _green = new GreenSyntaxList<GreenNode>(green);
        _position = position;
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, SyntaxTrivia> AsValueEnumerable() => new(GetEnumerator());

    public struct Enumerator : IEnumerator<SyntaxTrivia>, IValueEnumerator<SyntaxTrivia>
    {
        private readonly SyntaxTriviaList _list;
        private int _index;

        internal Enumerator(in SyntaxTriviaList list)
        {
            _list = list;
            _index = -1;
        }

        public SyntaxTrivia Current { get; private set; }
        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_index >= _list.Count)
                return false;

            ++_index;
            if (_index >= _list.Count)
                return false;

            Current = _list[_index];
            return true;
        }

        public bool TryGetNext(out SyntaxTrivia current)
        {
            if (!MoveNext())
            {
                current = default;
                return false;
            }

            current = Current;
            return true;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _list.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<SyntaxTrivia> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<SyntaxTrivia> destination, Index offset)
        {
            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}
