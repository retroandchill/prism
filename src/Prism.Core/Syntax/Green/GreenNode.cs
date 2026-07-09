// @file GreenNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal abstract class GreenNode
{
    public SyntaxKind Kind { get; }

    public int FullWidth { get; }

    public virtual int Width => FullWidth - LeadingTriviaWidth - TrailingTriviaWidth;

    public SyntaxFlags Flags { get; init; } = SyntaxFlags.None;

    public bool IsMissing => Flags.HasFlag(SyntaxFlags.Missing);

    public bool ContainsDiagnostics => Flags.HasFlag(SyntaxFlags.ContainsDiagnostics);

    public virtual int LeadingTriviaWidth => FullWidth > 0 ? GetFirstLeaf()!.LeadingTriviaWidth : 0;

    public bool HasLeadingTrivia => LeadingTriviaWidth > 0;

    public virtual int TrailingTriviaWidth =>
        FullWidth > 0 ? GetLastLeaf()!.TrailingTriviaWidth : 0;

    public bool HasTrailingTrivia => TrailingTriviaWidth > 0;

    public int ChildCount { get; protected init; }

    public ImmutableArray<DiagnosticInfo> Diagnostics
    {
        get;
        init
        {
            field = !value.IsDefault ? value : [];
            if (!field.IsEmpty)
            {
                Flags |= SyntaxFlags.ContainsDiagnostics;
            }
        }
    }

    protected GreenNode(SyntaxKind kind, int fullWidth)
    {
        Kind = kind;
        FullWidth = fullWidth;
    }

    public GreenNode? GetFirstLeaf()
    {
        var node = this;

        do
        {
            GreenNode? firstChild = null;
            for (int i = 0, n = node.ChildCount; i < n; i++)
            {
                var child = node.GetChild(i);
                if (child is null)
                    continue;

                firstChild = child;
                break;
            }

            node = firstChild;
        } while (node?.ChildCount > 0);

        return node;
    }

    public GreenNode? GetLastLeaf()
    {
        var node = this;

        do
        {
            GreenNode? lastChild = null;

            for (var i = node.ChildCount - 1; i >= 0; i--)
            {
                var child = GetChild(i);
                if (child is null)
                    continue;

                lastChild = child;
                break;
            }

            node = lastChild;
        } while (node?.ChildCount > 0);

        return node;
    }

    public abstract GreenNode? GetChild(int index);

    public int GetChildOffset(int index)
    {
        var offset = 0;
        for (var i = 0; i < index; i++)
        {
            var child = GetChild(i);
            if (child is not null)
            {
                offset += child.FullWidth;
            }
        }

        return offset;
    }

    public T? GetChild<T>(int index)
        where T : GreenNode
    {
        return GetChild(index) as T;
    }

    public GreenNode GetRequiredChild(int index)
    {
        return GetChild(index)
            ?? throw new InvalidOperationException($"Child at index {index} is null");
    }

    public T GetRequiredChild<T>(int index)
        where T : GreenNode
    {
        return GetChild<T>(index)
            ?? throw new InvalidOperationException(
                $"Child at index {index} is not an instance of {typeof(T)}"
            );
    }
}
