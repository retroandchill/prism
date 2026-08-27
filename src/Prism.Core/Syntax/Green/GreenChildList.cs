// @file GreenChildList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using ZLinq;

namespace Prism.Core.Syntax.Green;

internal struct GreenChildList(GreenNode? node)
    : IReadOnlyCollection<GreenNode>,
        IValueEnumerable<GreenChildList.Enumerator, GreenNode>
{
    private int? _count;

    public int Count
    {
        get
        {
            if (_count is not null)
                return _count.Value;

            _count = CountNodes();
            return _count.Value;
        }
    }

    public Enumerator GetEnumerator() => new(node);

    IEnumerator<GreenNode> IEnumerable<GreenNode>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, GreenNode> AsValueEnumerable() => new(GetEnumerator());

    public Reversed Reverse() => new(node);

    private int CountNodes()
    {
        return AsValueEnumerable().Count();
    }

    public struct Enumerator(GreenNode? node) : IEnumerator<GreenNode>, IValueEnumerator<GreenNode>
    {
        public GreenNode? Node { get; } = node;
        private int _slotIndex = -1;
        private int _listIndex = -1;
        private GreenNode? _list = null;
        private GreenNode? _current = null;

        public GreenNode Current =>
            _current ?? throw new InvalidOperationException("Enumerator has not been started yet");

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (Node is not null)
            {
                if (_list is not null)
                {
                    _listIndex++;

                    if (_listIndex < _list.SlotCount)
                    {
                        _current = _list.GetSlot(_listIndex);
                        return true;
                    }

                    _list = null;
                    _listIndex = -1;
                }

                while (true)
                {
                    _slotIndex++;

                    if (_slotIndex == Node.SlotCount)
                        break;

                    var child = Node.GetSlot(_slotIndex);
                    if (child is null)
                        continue;

                    if (child.IsList)
                    {
                        _list = child;
                        _listIndex++;

                        if (_listIndex < _list.SlotCount)
                        {
                            _current = _list.GetSlot(_listIndex);
                            return true;
                        }

                        _list = null;
                        _listIndex = -1;
                        continue;
                    }

                    _current = child;
                    return true;
                }
            }

            _current = null;
            return false;
        }

        public void Reset()
        {
            _slotIndex = -1;
            _list = null;
            _listIndex = -1;
            _current = null;
        }

        public bool TryGetNext(out GreenNode current)
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
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<GreenNode> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<GreenNode> destination, Index offset)
        {
            return false;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }

    public readonly struct Reversed(GreenNode? node)
        : IEnumerable<GreenNode>,
            IValueEnumerable<ReversedEnumerator, GreenNode>
    {
        public ReversedEnumerator GetEnumerator() => new(node);

        IEnumerator<GreenNode> IEnumerable<GreenNode>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ValueEnumerable<ReversedEnumerator, GreenNode> AsValueEnumerable() =>
            new(GetEnumerator());
    }

    public struct ReversedEnumerator : IEnumerator<GreenNode>, IValueEnumerator<GreenNode>
    {
        public GreenNode? Node { get; }
        private int _childIndex;
        private GreenNode? _list;
        private int _listIndex;
        private GreenNode? _currentChild;

        public ReversedEnumerator(GreenNode? node)
        {
            if (node is not null)
            {
                Node = node;
                _childIndex = node.SlotCount;
            }
            else
            {
                Node = null;
                _childIndex = 0;
            }

            _listIndex = -1;
            _list = null;
            _currentChild = null;
        }

        public GreenNode Current =>
            _currentChild ?? throw new InvalidOperationException("Enumerable is not started");

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (Node is not null)
            {
                if (_list is not null)
                {
                    if (--_listIndex >= 0)
                    {
                        _currentChild = _list.GetSlot(_listIndex);
                        return true;
                    }

                    _list = null;
                    _listIndex = -1;
                }

                while (--_childIndex >= 0)
                {
                    var child = Node.GetSlot(_childIndex);
                    if (child is null)
                    {
                        continue;
                    }

                    if (child.IsList)
                    {
                        _list = child;
                        _listIndex = _list.SlotCount;
                        if (--_listIndex >= 0)
                        {
                            _currentChild = _list.GetSlot(_listIndex);
                            return true;
                        }

                        _list = null;
                        _listIndex = -1;
                        continue;
                    }

                    _currentChild = child;
                    return true;
                }
            }

            _currentChild = null;
            return false;
        }

        public void Reset()
        {
            _childIndex = -1;
            _list = null;
            _listIndex = 0;
            _currentChild = null;
        }

        public bool TryGetNext(out GreenNode current)
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
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<GreenNode> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<GreenNode> destination, Index offset)
        {
            return false;
        }

        public void Dispose()
        {
            // Nothing to dispose of
        }
    }
}

internal static class GreenChildListExtensions
{
    extension(ValueEnumerable<GreenChildList.Enumerator, GreenNode> enumerable)
    {
        public ValueEnumerable<GreenChildList.ReversedEnumerator, GreenNode> Reverse()
        {
            var list = new GreenChildList(enumerable.Enumerator.Node);
            return list.Reverse().AsValueEnumerable();
        }
    }

    extension(ValueEnumerable<GreenChildList.ReversedEnumerator, GreenNode> enumerable)
    {
        public ValueEnumerable<GreenChildList.Enumerator, GreenNode> Reverse()
        {
            var list = new GreenChildList(enumerable.Enumerator.Node);
            return list.AsValueEnumerable();
        }
    }
}
