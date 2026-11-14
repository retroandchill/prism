// @file ReadOnlyList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using ZLinq;
using ZLinq.Linq;

namespace Prism.Utils;

public readonly struct ReadOnlyList<T>(IReadOnlyList<T> list)
    : IReadOnlyList<T>,
        IValueEnumerable<FromEnumerable<T>, T>
{
    public int Count => list.Count;

    public T this[int index] => list[index];

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();

    public ValueEnumerable<FromEnumerable<T>, T> AsValueEnumerable() => list.AsValueEnumerable();
}

public static class ReadOnlyList
{
    public static ReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyList<T> list) => new(list);
}
