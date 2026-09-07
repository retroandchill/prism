// @file SyntaxDiagnosticInfoList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using Prism.Core.Diagnostics;
using ZLinq;

namespace Prism.Core.Syntax.Green;

internal readonly struct SyntaxDiagnosticInfoList(GreenNode node)
    : IEnumerable<SyntaxDiagnosticInfo>,
        IValueEnumerable<SyntaxDiagnosticInfoList.Enumerator, SyntaxDiagnosticInfo>
{
    public Enumerator GetEnumerator() => new(node);

    IEnumerator<SyntaxDiagnosticInfo> IEnumerable<SyntaxDiagnosticInfo>.GetEnumerator() =>
        GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, SyntaxDiagnosticInfo> AsValueEnumerable() =>
        new(GetEnumerator());

    public struct Enumerator
        : IEnumerator<SyntaxDiagnosticInfo>,
            IValueEnumerator<SyntaxDiagnosticInfo>
    {
        private readonly record struct NodeIteration(
            GreenNode Node,
            int DiagnosticIndex = -1,
            int SlotIndex = -1
        );

        private readonly List<NodeIteration>? _stack;

        public SyntaxDiagnosticInfo Current { get; private set; }

        object IEnumerator.Current => Current;

        public Enumerator(GreenNode? node)
        {
            if (node is not { ContainsDiagnostics: true })
                return;

            _stack = [new NodeIteration(node)];
        }

        public bool MoveNext()
        {
            while (_stack is not null && _stack.Count > 0)
            {
                var (node, diagIndex, slotIndex) = _stack[^1];
                var diagnostics = node.Diagnostics;
                if (diagIndex < diagnostics.Length - 1)
                {
                    diagIndex++;
                    Current = diagnostics[diagIndex];
                    _stack[^1] = _stack[^1] with { DiagnosticIndex = diagIndex };
                    return true;
                }

                while (true)
                {
                    if (slotIndex < node.SlotCount - 1)
                    {
                        slotIndex++;
                        var child = node.GetSlot(slotIndex);
                        if (child is not { ContainsDiagnostics: true })
                        {
                            continue;
                        }

                        _stack[^1] = _stack[^1] with { SlotIndex = slotIndex };
                        _stack.Add(new NodeIteration(child));
                    }
                    else
                    {
                        _stack.RemoveAt(_stack.Count - 1);
                    }

                    break;
                }
            }

            return false;
        }

        public bool TryGetNext(out SyntaxDiagnosticInfo current)
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
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<SyntaxDiagnosticInfo> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<SyntaxDiagnosticInfo> destination, Index offset)
        {
            return false;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            // Do nothing
        }
    }
}
