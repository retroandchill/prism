// @file SyntaxNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;
using Prism.Core.Text;
using ZLinq;

namespace Prism.Core.Syntax;

public abstract class SyntaxNode
{
    private protected SyntaxNode(GreenNode green, SyntaxNode? parent, int position)
    {
        Green = green;
        Parent = parent;
        Position = position;
    }

    internal GreenNode Green { get; }
    public SyntaxNode? Parent { get; }
    public SyntaxKind Kind => Green.Kind;
    internal int Position { get; }
    internal int EndPosition => Position + Green.FullWidth;
    public TextSpan FullSpan => new(Position, Green.FullWidth);
    public TextSpan Span => new(Position + Green.LeadingTriviaWidth, Green.Width);
    public int SpanStart => Position + Green.LeadingTriviaWidth;
    public int Width => Green.Width;

    public bool IsMissing => Green.IsMissing;
    public bool ContainsDiagnostics => Green.ContainsDiagnostics;

    public bool HasLeadingTrivia => Green.HasLeadingTrivia;

    public SyntaxTriviaList LeadingTrivia => FirstToken.LeadingTrivia;
    public bool HasTrailingTrivia => Green.HasTrailingTrivia;

    public SyntaxTriviaList TrailingTrivia => LastToken.TrailingTrivia;

    private SyntaxTree? _syntaxTree;
    public SyntaxTree SyntaxTree
    {
        get
        {
            var result = _syntaxTree ?? ComputeSyntaxTree(this);
            Debug.Assert(result is not null);
            return result;
        }
        internal init => _syntaxTree = value;
    }

    private static SyntaxTree ComputeSyntaxTree(SyntaxNode node)
    {
        List<SyntaxNode>? nodes = null;
        SyntaxTree? tree;

        while (true)
        {
            tree = node._syntaxTree;
            if (tree is not null)
                break;

            var parent = node.Parent;
            if (parent is null)
            {
                Interlocked.CompareExchange(ref node._syntaxTree, new SyntaxTree(node), null);
                tree = node._syntaxTree;
                break;
            }

            tree = parent._syntaxTree;
            if (tree is not null)
            {
                node._syntaxTree = tree;
                break;
            }

            nodes ??= [node];
            node = parent;
        }

        if (nodes is null)
            return tree;

        Debug.Assert(tree is not null);

        foreach (var n in nodes)
        {
            var existingTree = n._syntaxTree;
            if (existingTree is not null)
            {
                Debug.Assert(existingTree == tree, "This should not happen");
                break;
            }
            n._syntaxTree = tree;
        }

        return tree;
    }

    public Location Location
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.Exchange(ref field, new SourceLocation(this));
            return field;
        }
    }

    internal abstract SyntaxNode? GetNodeSlot(int index);

    internal SyntaxNode GetRequiredNodeSlot(int index)
    {
        return GetNodeSlot(index) ?? throw new InvalidOperationException("Node slot is null");
    }

    internal SyntaxNode? GetRed(ref SyntaxNode? slot)
    {
        var result = slot;

        if (result is not null)
            return result;
        var green = Green.GetSlot(0);
        if (green == null)
            return result;
        Interlocked.CompareExchange(ref slot, green.CreateRed(this, Position), null);
        result = slot;

        return result;
    }

    internal SyntaxNode? GetRed(ref SyntaxNode? slot, int index)
    {
        var result = slot;

        if (result is not null)
            return result;
        var green = Green.GetSlot(index);
        if (green == null)
            return result;
        Interlocked.CompareExchange(ref slot, green.CreateRed(this, GetSlotPosition(index)), null);
        result = slot;

        return result;
    }

    internal T? GetRed<T>(ref T? slot)
        where T : SyntaxNode
    {
        var result = slot;

        if (result is not null)
            return result;
        var green = Green.GetSlot(0);
        if (green == null)
            return result;
        Interlocked.CompareExchange(ref slot, (T)green.CreateRed(this, Position), null);
        result = slot;

        return result;
    }

    internal T? GetRed<T>(ref T? slot, int index)
        where T : SyntaxNode
    {
        var result = slot;

        if (result is not null)
            return result;
        var green = Green.GetSlot(index);
        if (green == null)
            return result;
        Interlocked.CompareExchange(
            ref slot,
            (T)green.CreateRed(this, GetSlotPosition(index)),
            null
        );
        result = slot;

        return result;
    }

    internal abstract SyntaxNode? GetCachedSlot(int index);

    internal int GetSlotPosition(int index)
    {
        var cached = GetCachedSlot(index);
        if (cached is not null)
            return cached.Position;

        var offset = 0;
        var green = Green;
        while (index > 0)
        {
            index--;
            var prevSibling = GetCachedSlot(index);
            if (prevSibling is not null)
                return prevSibling.EndPosition + offset;

            var greenChild = green.GetSlot(index);
            if (greenChild is not null)
                offset += greenChild.FullWidth;
        }

        return Position + offset;
    }

    public ChildSyntaxList ChildNodesAndTokens() => new(this);

    public IEnumerable<SyntaxNode> ChildNodes()
    {
        foreach (var child in ChildNodesAndTokens())
        {
            if (child.TryGetValue(out SyntaxNode? node))
                yield return node;
        }
    }

    public IEnumerable<SyntaxToken> ChildTokens()
    {
        foreach (var child in ChildNodesAndTokens())
        {
            if (child.TryGetValue(out SyntaxToken token))
                yield return token;
        }
    }

    public SyntaxToken FirstToken
    {
        get
        {
            var first = ChildNodesAndTokens().AsValueEnumerable().First();
            SyntaxToken token;
            while (!first.TryGetValue(out token))
            {
                first = first.AsSyntaxNode().ChildNodesAndTokens().AsValueEnumerable().First();
            }

            return token;
        }
    }

    public SyntaxToken LastToken
    {
        get
        {
            var last = ChildNodesAndTokens().Reverse().AsValueEnumerable().First();
            SyntaxToken token;
            while (!last.TryGetValue(out token))
            {
                last = last.AsSyntaxNode()
                    .ChildNodesAndTokens()
                    .Reverse()
                    .AsValueEnumerable()
                    .First();
            }

            return token;
        }
    }
}
