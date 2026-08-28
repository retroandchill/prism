// @file GreenNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenNode(SyntaxKind kind, int fullWidth = 0)
{
    public SyntaxKind Kind { get; } = kind;

    public bool IsList => Kind == SyntaxKind.List;

    public bool IsToken => Kind.IsToken;

    public bool IsTrivia => Kind.IsTrivia;

    public int FullWidth { get; private set; } = fullWidth;

    public virtual int Width => FullWidth - LeadingTriviaWidth - TrailingTriviaWidth;

    public SyntaxFlags Flags { get; private set; }

    protected void SetFlags(SyntaxFlags flags)
    {
        Flags |= flags;
    }

    protected void ClearFlags(SyntaxFlags flags)
    {
        Flags &= ~flags;
    }

    protected void AdjustFlagsAndWidth(GreenNode node)
    {
        SetFlags(node.Flags & SyntaxFlags.InheritMask);
        FullWidth += node.FullWidth;
    }

    public bool IsMissing => !Flags.HasFlag(SyntaxFlags.NotMissing);

    public bool ContainsDiagnostics => Flags.HasFlag(SyntaxFlags.ContainsDiagnostics);

    public virtual GreenNode? LeadingTrivia => FullWidth != 0 ? FirstTerminal!.LeadingTrivia : null;

    public virtual int LeadingTriviaWidth => FullWidth > 0 ? FirstTerminal!.LeadingTriviaWidth : 0;

    public bool HasLeadingTrivia => LeadingTriviaWidth > 0;

    public virtual GreenNode? TrailingTrivia =>
        FullWidth != 0 ? LastTerminal!.TrailingTrivia : null;

    public virtual int TrailingTriviaWidth => FullWidth > 0 ? LastTerminal!.TrailingTriviaWidth : 0;

    public bool HasTrailingTrivia => TrailingTriviaWidth > 0;

    public GreenNode? FirstTerminal
    {
        get
        {
            var node = this;

            do
            {
                GreenNode? firstChild = null;

                var slotCount = node.SlotCount;
                for (var i = 0; i < slotCount; i++)
                {
                    var child = node.GetSlot(i);
                    if (child is null)
                        continue;

                    firstChild = child;
                    break;
                }

                node = firstChild;
            } while (node is not null && node.SlotCount > 0);

            return node;
        }
    }

    public GreenNode? LastTerminal
    {
        get
        {
            var node = this;

            do
            {
                GreenNode? lastChild = null;

                for (var i = node.SlotCount - 1; i >= 0; i--)
                {
                    var child = node.GetSlot(i);
                    if (child is null)
                        continue;

                    lastChild = child;
                    break;
                }

                node = lastChild;
            } while (node is not null && node.SlotCount > 0);

            return node;
        }
    }

    public int SlotCount { get; protected init; }

    public abstract GreenNode? GetSlot(int index);

    public T? GetSlot<T>(int index)
        where T : GreenNode
    {
        return GetSlot(index) as T;
    }

    public T GetRequiredSlot<T>(int index)
        where T : GreenNode
    {
        var node = GetSlot<T>(index);
        return node
            ?? throw new InvalidOperationException($"Slot {index} is required but not found");
    }

    public int GetSlotOffset(int index)
    {
        var offset = 0;
        for (var i = 0; i < index; i++)
        {
            var child = GetSlot(i);
            if (child is null)
                continue;

            offset += child.FullWidth;
        }

        return offset;
    }

    public GreenChildList ChildNodesAndTokens => new(this);

    public GreenNodeEnumerable EnumerateNodes() => new(this);

    public ImmutableArray<SyntaxDiagnosticInfo> Diagnostics { get; init; } = [];

    public abstract GreenNode WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics);

    public abstract SyntaxNode CrateRed(SyntaxNode? parent, int positon = 0);

    public virtual void WriteTo(TextWriter writer)
    {
        for (var i = 0; i < SlotCount; i++)
        {
            var child = GetSlot(i);
            child?.WriteTo(writer);
        }
    }

    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}
