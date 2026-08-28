using System.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public readonly struct ChildSyntaxList(SyntaxNode node)
{
    private readonly record struct SlotData(
        int SlotIndex = 0,
        int PrecedingOccupantSlotCount = 0,
        int PositionAtSlotIndex = 0
    );

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
}
