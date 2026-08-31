using System.Collections;
using System.Collections.Immutable;
using ZLinq;
using ZLinq.Linq;

namespace Prism.Core.Utils;

public readonly struct ImmutableOrderedSet<T>
    : IReadOnlyCollection<T>,
        IValueEnumerable<FromImmutableArray<T>, T>
{
    private readonly ImmutableArray<T> _values;
    private readonly ImmutableHashSet<T> _set;

    private ImmutableOrderedSet(ImmutableArray<T> values, ImmutableHashSet<T> set)
    {
        _values = values;
        _set = set;
    }

    public static readonly ImmutableOrderedSet<T> Empty = new([], ImmutableHashSet<T>.Empty);

    public int Count => _values.Length;

    public ImmutableOrderedSet<T> Add(T value)
    {
        var nextSet = _set.Add(value);
        return _set == nextSet ? this : new ImmutableOrderedSet<T>(_values.Add(value), nextSet);
    }

    public ImmutableOrderedSet<T> AddRange(IEnumerable<T> values)
    {
        var changed = false;
        ImmutableArray<T>.Builder? newValues = null;
        ImmutableHashSet<T>.Builder? explored = null;

        foreach (var value in values)
        {
            if (!changed)
            {
                if (_set.Contains(value))
                    continue;

                newValues ??= ImmutableArray.CreateBuilder<T>();
                explored ??= ImmutableHashSet.CreateBuilder<T>();
                newValues.AddRange(_values);
                explored.UnionWith(_values);
                changed = true;
            }
            else if (explored?.Contains(value) ?? false)
                continue;

            newValues ??= ImmutableArray.CreateBuilder<T>();
            explored ??= ImmutableHashSet.CreateBuilder<T>();
            newValues.Add(value);
            explored.Add(value);
        }

        return !changed
            ? this
            : new ImmutableOrderedSet<T>(newValues!.ToImmutable(), explored!.ToImmutableHashSet());
    }

    public ImmutableOrderedSet<T> Remove(T value)
    {
        var newSet = _set.Remove(value);
        return _set == newSet
            ? this
            : new ImmutableOrderedSet<T>(RemoveOrderedValue(value), newSet);
    }

    public ImmutableOrderedSet<T> RemoveRange(IEnumerable<T> values)
    {
        var changed = false;
        ImmutableArray<T>.Builder? newValues = null;

        foreach (var value in values)
        {
            if (!changed)
            {
                if (_set.Contains(value))
                    continue;

                newValues ??= ImmutableArray.CreateBuilder<T>();
                newValues.AddRange(_values);
                changed = true;
            }
            else if (newValues is null)
                continue;

            newValues.Remove(value);
        }

        return !changed
            ? this
            : new ImmutableOrderedSet<T>(newValues!.ToImmutable(), newValues.ToImmutableHashSet());
    }

    public ImmutableArray<T>.Enumerator GetEnumerator() => _values.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_values).GetEnumerator();

    private ImmutableArray<T> RemoveOrderedValue(T value)
    {
        foreach (var (i, v) in _values.AsValueEnumerable().Index())
        {
            if (_set.KeyComparer.Equals(v, value))
                return _values.RemoveAt(i);
        }

        return _values;
    }

    public ValueEnumerable<FromImmutableArray<T>, T> AsValueEnumerable() =>
        _values.AsValueEnumerable();
}
