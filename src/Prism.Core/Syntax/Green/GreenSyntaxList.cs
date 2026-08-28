using System.Collections;
using System.Numerics;
using ZLinq;

namespace Prism.Core.Syntax.Green;

internal readonly struct GreenSyntaxList<T>
    : IGreenNodeWrapper,
        IEquatable<GreenSyntaxList<T>>,
        IEqualityOperators<GreenSyntaxList<T>, GreenSyntaxList<T>, bool>,
        IReadOnlyList<T>,
        IValueEnumerable<GreenSyntaxList<T>.Enumerator, T>
    where T : GreenNode
{
    private readonly GreenNode? _children;

    public GreenSyntaxList(GreenNode? children)
    {
        _children = children;
    }

    public int Count =>
        _children switch
        {
            GreenListNode list => list.SlotCount,
            null => 0,
            _ => 1,
        };

    public T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            if (_children is null)
                throw new IndexOutOfRangeException("List is empty");

            if (_children is not GreenListNode list)
            {
                if (index != 0)
                    throw new IndexOutOfRangeException("Index out of range");
                return (T)_children;
            }

            var child = list.GetSlot(index);
            if (child is null)
                throw new IndexOutOfRangeException("Index out of range");

            return (T)child;
        }
    }

    GreenNode? IGreenNodeWrapper.Node => _children;

    public override bool Equals(object? obj)
    {
        return obj is GreenSyntaxList<T> other && Equals(other);
    }

    public bool Equals(GreenSyntaxList<T> other)
    {
        return _children == other._children;
    }

    public override int GetHashCode()
    {
        return _children?.GetHashCode() ?? 0;
    }

    public static bool operator ==(GreenSyntaxList<T> left, GreenSyntaxList<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GreenSyntaxList<T> left, GreenSyntaxList<T> right)
    {
        return !left.Equals(right);
    }

    public void WriteTo(TextWriter writer)
    {
        _children?.WriteTo(writer);
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
        private readonly GreenListNode.Builder _builder = new();

        public int Count => _builder.Count;

        public void Add(T node)
        {
            _builder.Add(node);
        }

        public GreenSyntaxList<T> Build()
        {
            return new GreenSyntaxList<T>(_builder.Build());
        }

        public GreenSyntaxList<T> BuildAndClear()
        {
            return new GreenSyntaxList<T>(_builder.BuildAndClear());
        }
    }

    public struct Enumerator(GreenSyntaxList<T> list) : IEnumerator<T>, IValueEnumerator<T>
    {
        private readonly GreenSyntaxList<T> _list = list;
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

internal static class GreenSyntaxList
{
    public static GreenSyntaxList<T>.Builder CreateBuilder<T>()
        where T : GreenNode
    {
        return new GreenSyntaxList<T>.Builder();
    }
}
