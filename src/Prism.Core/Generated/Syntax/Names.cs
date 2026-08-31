// Generated file, do not edit
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public abstract class NameSyntax : SyntaxNode
{
    private protected NameSyntax(GreenName node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }
};

public sealed class SimpleNameSyntax : NameSyntax
{
    internal SimpleNameSyntax(GreenSimpleName node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public SyntaxToken Identifier
    {
        get { return new SyntaxToken(((GreenSimpleName)Green).Identifier, this, Position); }
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

public sealed class QualifiedNameSyntax : NameSyntax
{
    internal QualifiedNameSyntax(GreenQualifiedName node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    private NameSyntax? _left;
    public NameSyntax Left
    {
        get { return GetRed(ref _left); }
    }
    public SyntaxToken Separator
    {
        get
        {
            return new SyntaxToken(((GreenQualifiedName)Green).Separator, this, GetSlotPosition(1));
        }
    }
    private SimpleNameSyntax? _right;
    public SimpleNameSyntax Right
    {
        get { return GetRed(ref _right, 2); }
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index switch
        {
            0 => GetRed(ref _left),
            2 => GetRed(ref _right, 2),
            _ => null,
        };
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return index switch
        {
            0 => _left,
            2 => _right,
            _ => null,
        };
    }
};
