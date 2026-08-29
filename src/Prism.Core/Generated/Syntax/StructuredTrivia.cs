// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
    internal SkippedTokensTriviaSyntax(
        GreenSkippedTokensTrivia node,
        SyntaxNode? parent,
        int position
    )
        : base(node, parent, position) { }

    public SyntaxTokenList Tokens
    {
        get
        {
            return new SyntaxTokenList(this, ((GreenSkippedTokensTrivia)Green).Tokens, Position);
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return null;
    }
};
