using System.Collections;
using System.Diagnostics;
using Prism.Core.Syntax.Green;
using ZLinq;

namespace Prism.Core.Syntax;

public readonly struct ChildSyntaxList(SyntaxNode node)
    : IReadOnlyList<SyntaxNodeOrToken>,
        IValueEnumerable<ChildSyntaxList.Enumerator, SyntaxNodeOrToken>
{
    private readonly record struct SlotData(
        int SlotIndex = 0,
        int PrecedingOccupantSlotCount = 0,
        int PositionAtSlotIndex = 0
    )
    {
        public SlotData(SyntaxNode node)
            : this(SlotIndex: 0, PrecedingOccupantSlotCount: 0, PositionAtSlotIndex: node.Position)
        { }
    }

    private readonly SyntaxNode? _node = node;
    public int Count { get; } = CountNodes(node.Green);

    private static int CountNodes(GreenNode green)
    {
        var n = 0;
        for (int i = 0, s = green.SlotCount; i < s; i++)
        {
            var child = green.GetSlot(i);
            if (child is null)
                continue;

            if (!child.IsList)
                n++;
            else
                n += child.SlotCount;
        }

        return n;
    }

    public SyntaxNodeOrToken this[int index] =>
        unchecked((uint)index < (uint)Count)
            ? GetItem(_node!, index)
            : throw new IndexOutOfRangeException("Invalid index");

    private static SyntaxNodeOrToken GetItem(SyntaxNode node, int index)
    {
        var slotData = new SlotData(PositionAtSlotIndex: node.Position);
        return GetItem(node, index, ref slotData);
    }

    private static SyntaxNodeOrToken GetItem(SyntaxNode node, int index, ref SlotData slotData)
    {
        GreenNode? greenChild;
        var green = node.Green;

        Debug.Assert(index > slotData.PrecedingOccupantSlotCount);
        var idx = index = slotData.PrecedingOccupantSlotCount;
        var slotIndex = slotData.SlotIndex;
        var position = slotData.PositionAtSlotIndex;

        while (true)
        {
            greenChild = green.GetSlot(slotIndex);
            if (greenChild is not null)
            {
                var currentOccupancy = Occupancy(greenChild);
                if (idx < currentOccupancy)
                    break;

                idx -= currentOccupancy;
                position += greenChild.FullWidth;
            }

            slotIndex++;
        }

        if (slotIndex != slotData.SlotIndex)
        {
            slotData = new SlotData(slotIndex, index - idx, position);
        }

        var red = node.GetNodeSlot(slotIndex);
        if (!greenChild.IsList)
        {
            if (red is not null)
                return red;
        }
        else if (red is not null)
        {
            var redChild = red.GetNodeSlot(idx);
            if (redChild is not null)
                return redChild;

            greenChild = green.GetSlot(idx);
            position = red.GetSlotPosition(idx);
        }
        else
        {
            position += greenChild.GetSlotOffset(idx);
            greenChild = greenChild.GetSlot(idx);
        }

        return new SyntaxToken((GreenToken)greenChild!, node, position);
    }

    private static int Occupancy(GreenNode node)
    {
        return node.IsList ? node.SlotCount : 1;
    }

    public Enumerator GetEnumerator() => new(_node!, Count);

    IEnumerator<SyntaxNodeOrToken> IEnumerable<SyntaxNodeOrToken>.GetEnumerator() =>
        GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ValueEnumerable<Enumerator, SyntaxNodeOrToken> AsValueEnumerable() =>
        new(GetEnumerator());

    public struct Enumerator : IEnumerator<SyntaxNodeOrToken>, IValueEnumerator<SyntaxNodeOrToken>
    {
        private readonly SyntaxNode? _node;
        private readonly int _count;
        private int _childIndex;
        private SlotData _slotData;

        internal Enumerator(SyntaxNode node, int count)
        {
            _node = node;
            _count = count;
            _childIndex = -1;
            _slotData = new SlotData(node);
        }

        public SyntaxNodeOrToken Current
        {
            get
            {
                Debug.Assert(_node is not null);
                return GetItem(_node, _childIndex, ref _slotData);
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            var newIndex = _childIndex + 1;
            if (newIndex >= _count)
                return false;
            _childIndex = newIndex;
            Debug.Assert(_node is not null);
            return true;
        }

        public bool TryGetNext(out SyntaxNodeOrToken current)
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
            count = _count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<SyntaxNodeOrToken> span)
        {
            span = [];
            return false;
        }

        public bool TryCopyTo(scoped Span<SyntaxNodeOrToken> destination, Index offset)
        {
            return false;
        }

        public void Reset()
        {
            _childIndex = -1;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
