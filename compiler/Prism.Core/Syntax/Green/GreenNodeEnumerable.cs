// @file GreenNodeEnumerable.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using ZLinq;

namespace Prism.Core.Syntax.Green;

internal readonly ref struct GreenNodeEnumerable(GreenNode node)
{
    public Enumerator GetEnumerator() => new(node);

    public ValueEnumerable<Enumerator, GreenNode> AsValueEnumerable() => new(GetEnumerator());

    public ref struct Enumerator : IEnumerator<GreenNode>, IValueEnumerator<GreenNode>
    {
        private readonly Stack<GreenChildList.Enumerator> _stack;
        private bool _started;
        private GreenNode _current;

        public Enumerator(GreenNode node)
        {
            _current = node;
            _stack = [];
            _stack.Push(node.ChildNodesAndTokens.GetEnumerator());
        }

        public GreenNode Current => _current;

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                return true;
            }

            while (_stack.TryPop(out var enumerator))
            {
                if (!enumerator.MoveNext())
                {
                    enumerator.Dispose();
                    continue;
                }

                _current = enumerator.Current;

                _stack.Push(enumerator);

                if (_current.IsToken)
                    _stack.Push(_current.ChildNodesAndTokens.GetEnumerator());

                return true;
            }

            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public bool TryGetNext(out GreenNode current)
        {
            if (!MoveNext())
            {
                current = null!;
                return false;
            }

            current = _current;
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
            while (_stack.TryPop(out var enumerator))
                enumerator.Dispose();
        }
    }
}
