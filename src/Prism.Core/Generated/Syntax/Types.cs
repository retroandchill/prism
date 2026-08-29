// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public abstract class TypeSyntax : SyntaxNode
{
    private protected TypeSyntax(GreenType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }
};

public sealed class PredefinedTypeSyntax : TypeSyntax
{
    internal PredefinedTypeSyntax(GreenPredefinedType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Keyword
    {
        get { return new SyntaxToken(((GreenPredefinedType)Green).Keyword, this, Position); }
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

public sealed class NamedTypeSyntax : TypeSyntax
{
    internal NamedTypeSyntax(GreenNamedType node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private NameSyntax? _identifier;
    public NameSyntax Identifier
    {
        get { return GetRed(ref _identifier); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _identifier) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 0 ? _identifier : null;
    }
};
