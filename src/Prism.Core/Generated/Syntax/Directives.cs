// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public sealed class UsingDirectiveSyntax : SyntaxNode
{
    internal UsingDirectiveSyntax(GreenUsingDirective node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken UsingKeyword
    {
        get { return new SyntaxToken(((GreenUsingDirective)Green).UsingKeyword, this, Position); }
    }
    private NameSyntax? _name;
    public NameSyntax Name
    {
        get { return GetRed(ref _name, 1); }
    }
    public SyntaxToken Semicolon
    {
        get
        {
            return new SyntaxToken(
                ((GreenUsingDirective)Green).Semicolon,
                this,
                GetSlotPosition(2)
            );
        }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 1 ? GetRed(ref _name) : null;
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index == 1 ? _name : null;
    }
};
