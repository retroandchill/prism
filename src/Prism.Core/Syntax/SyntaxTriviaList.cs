using System.Collections;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public readonly struct SyntaxTriviaList : IReadOnlyList<SyntaxTrivia>
{
    public static SyntaxTriviaList Empty => new(default, GreenTriviaList.Empty);

    private readonly SyntaxToken _token;
    private readonly GreenTriviaList _list;
    private readonly int _position;

    public int Count => _list.ChildCount;

    public SyntaxTrivia this[int index]
    {
        get
        {
            var element = _list.GetChild(index);
            return element is not null
                ? new SyntaxTrivia(_token, element, _position + element.GetChildOffset(index))
                : throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    internal SyntaxTriviaList(in SyntaxToken token, GreenTriviaList list)
    {
        _token = token;
        _list = list;
        _position = token.Position;
    }

    internal SyntaxTriviaList(in SyntaxToken token, GreenTriviaList list, int position)
    {
        _token = token;
        _list = list;
        _position = position;
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<SyntaxTrivia>
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
