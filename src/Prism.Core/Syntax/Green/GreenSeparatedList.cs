using System.Collections;
using System.Diagnostics;
using System.Numerics;
using ZLinq;

namespace Prism.Core.Syntax.Green;

internal readonly struct GreenSeparatedList<T>
    : IGreenNodeWrapper,
        IEquatable<GreenSeparatedList<T>>,
        IEqualityOperators<GreenSeparatedList<T>, GreenSeparatedList<T>, bool>,
        IReadOnlyList<T>,
        IValueEnumerable<GreenSeparatedList<T>.Enumerator, T>
    where T : GreenNode
{
    private readonly GreenSyntaxList<GreenNode> _list;

    public int Count => (_list.Count + 1) / 2;

    public int SeparatorCount => _list.Count / 2;

    public GreenSeparatedList(GreenSyntaxList<GreenNode> list)
    {
        Validate(list);
        _list = list;
    }

    [Conditional("DEBUG")]
    private static void Validate(GreenSyntaxList<GreenNode> list)
    {
        foreach (var (i, item) in list.AsValueEnumerable().Index())
        {
            if (i % 2 == 0)
            {
                Debug.Assert(!item.IsToken);
            }
            else
            {
                Debug.Assert(item.IsToken);
            }
        }
    }

    public T this[int index] => (T)_list[index * 2];

    public GreenToken GetSeparator(int index) => (GreenToken)_list[index * 2 + 1];

    public GreenSyntaxList<GreenNode> WithSeparators() => _list;

    public GreenNode? Node => _list.Node;

    public override bool Equals(object? obj)
    {
        return obj is GreenSeparatedList<T> other && Equals(other);
    }

    public bool Equals(GreenSeparatedList<T> other)
    {
        return _list.Equals(other._list);
    }

    public override int GetHashCode()
    {
        return _list.GetHashCode();
    }

    public static bool operator ==(GreenSeparatedList<T> left, GreenSeparatedList<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GreenSeparatedList<T> left, GreenSeparatedList<T> right)
    {
        return !left.Equals(right);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, T> AsValueEnumerable() => new(GetEnumerator());

    public readonly struct Builder()
    {
        private readonly GreenSyntaxList<GreenNode>.Builder _builder =
            GreenSyntaxList.CreateBuilder<GreenNode>();

        public int Count => (_builder.Count + 1) / 2;

        public int SeparatorsCount => _builder.Count / 2;

        public void Add(T item)
        {
            _builder.Add(item);
        }

        public GreenSeparatedList<T> Build()
        {
            return new GreenSeparatedList<T>(_builder.Build());
        }

        public GreenSeparatedList<T> BuildAndClear()
        {
            return new GreenSeparatedList<T>(_builder.BuildAndClear());
        }
    }

    public struct Enumerator(GreenSeparatedList<T> list) : IEnumerator<T>, IValueEnumerator<T>
    {
        private readonly GreenSeparatedList<T> _list = list;
        private int _index = -1;

        public T Current { get; private set; }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_index >= _list.Count - 1)
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
            count = _list.Count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            // TODO: We may be able to use this
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

internal static class GreenSeparatedList
{
    public static GreenSeparatedList<T>.Builder CreateBuilder<T>()
        where T : GreenNode
    {
        return new GreenSeparatedList<T>.Builder();
    }
}
