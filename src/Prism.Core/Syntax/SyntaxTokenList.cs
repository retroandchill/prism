using System.Collections;
using Prism.Core.Syntax.Green;
using ZLinq;

namespace Prism.Core.Syntax;

public readonly struct SyntaxTokenList
    : IReadOnlyList<SyntaxToken>,
        IValueEnumerable<SyntaxTokenList.Enumerator, SyntaxToken>
{
    private readonly SyntaxNode _parent;
    private readonly GreenSyntaxList<GreenToken> _green;
    private readonly int _position;

    internal SyntaxTokenList(SyntaxNode parent, GreenSyntaxList<GreenToken> green, int position)
    {
        _parent = parent;
        _green = green;
        _position = position;
    }

    public int Count => _green.Count;

    public SyntaxToken this[int index] => new(_green[index], _parent, _position);

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxToken> IEnumerable<SyntaxToken>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, SyntaxToken> AsValueEnumerable() => new(GetEnumerator());

    public struct Enumerator : IEnumerator<SyntaxToken>, IValueEnumerator<SyntaxToken>
    {
        private readonly SyntaxTokenList _list;
        private int _index;

        internal Enumerator(in SyntaxTokenList list)
        {
            _list = list;
            _index = -1;
        }

        public SyntaxToken Current { get; private set; }
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

        public bool TryGetNext(out SyntaxToken current)
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

        public bool TryGetSpan(out ReadOnlySpan<SyntaxToken> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<SyntaxToken> destination, Index offset)
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
