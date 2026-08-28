// @file SyntaxNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;
using Prism.Core.Parse;
using Prism.Core.Syntax.Green;
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

    public SyntaxTree SyntaxTree => throw new NotImplementedException();

    public Location Location => throw new NotImplementedException();

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

    internal virtual int GetSlotPosition(int index)
    {
        throw new NotImplementedException();
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
