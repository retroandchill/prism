using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public static partial class TokenReplacer
{
    internal static partial GreenToken? ReplaceFirstToken(GreenToken? node, GreenToken newToken)
    {
        return node is not null ? newToken : null;
    }

    [DoesNotReturn]
    internal static partial GreenTrivia ReplaceFirstToken(GreenTrivia? node, GreenToken newToken)
    {
        throw new InvalidOperationException("Cannot replace first token in trivia.");
    }

    internal static partial GreenListNode? ReplaceFirstToken(
        GreenListNode? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        for (var i = 0; i < node.SlotCount; i++)
        {
            var oldNode = node.GetSlot(i);
            Debug.Assert(oldNode is not null);
            var newNode = ReplaceFirstToken(oldNode, newToken);
            if (oldNode != newNode)
                return node.WithSlot(i, newNode);
        }

        return node;
    }

    internal static GreenSyntaxList<T> ReplaceFirstToken<T>(
        GreenSyntaxList<T> node,
        GreenToken newToken
    )
        where T : GreenNode
    {
        return new GreenSyntaxList<T>(ReplaceFirstToken(node.Node, newToken));
    }

    internal static GreenSeparatedList<T> ReplaceFirstToken<T>(
        GreenSeparatedList<T> node,
        GreenToken newToken
    )
        where T : GreenNode
    {
        return new GreenSeparatedList<T>(ReplaceFirstToken(node.Node, newToken));
    }

    internal static partial GreenToken? ReplaceLastToken(GreenToken? node, GreenToken newToken)
    {
        return node is not null ? newToken : null;
    }

    [DoesNotReturn]
    internal static partial GreenTrivia ReplaceLastToken(GreenTrivia? node, GreenToken newToken)
    {
        throw new InvalidOperationException("Cannot replace first token in trivia.");
    }

    internal static partial GreenListNode? ReplaceLastToken(
        GreenListNode? node,
        GreenToken newToken
    )
    {
        if (node is null)
            return null;

        for (var i = node.SlotCount - 1; i >= 0; i--)
        {
            var oldNode = node.GetSlot(i);
            Debug.Assert(oldNode is not null);
            var newNode = ReplaceLastToken(oldNode, newToken);
            if (oldNode != newNode)
                return node.WithSlot(i, newNode);
        }

        return node;
    }

    internal static GreenSyntaxList<T> ReplaceLastToken<T>(
        GreenSyntaxList<T> node,
        GreenToken newToken
    )
        where T : GreenNode
    {
        return new GreenSyntaxList<T>(ReplaceLastToken(node.Node, newToken));
    }

    internal static GreenSeparatedList<T> ReplaceLastToken<T>(
        GreenSeparatedList<T> node,
        GreenToken newToken
    )
        where T : GreenNode
    {
        return new GreenSeparatedList<T>(ReplaceLastToken(node.Node, newToken));
    }
}
