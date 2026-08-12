// @file GreenChildList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using ZLinq;

namespace Prism.Core.Syntax.Green;

internal struct GreenChildList(GreenNode node)
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

    public ValueEnumerable<Enumerator, GreenNode> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, GreenNode>(GetEnumerator());
    }

    private int CountNodes()
    {
        return AsValueEnumerable().Count();
    }

    public struct Enumerator(GreenNode node) : IEnumerator<GreenNode>, IValueEnumerator<GreenNode>
    {
        private GreenNode? _node = node;
        private int _slotIndex = -1;
        private int _listIndex = -1;
        private GreenNode? _list = null;
        private GreenNode? _current = null;

        public GreenNode Current =>
            _current ?? throw new InvalidOperationException("Enumerator has not been started yet");

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_node is not null)
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

                    if (_slotIndex == _node.SlotCount)
                        break;

                    var child = _node.GetSlot(_slotIndex);
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
}
