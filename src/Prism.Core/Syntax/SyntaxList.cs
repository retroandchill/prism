using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

internal sealed class SyntaxList : SyntaxNode
{
    private readonly SyntaxNode?[] _children;

    public SyntaxList(GreenListNode node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
        _children = new SyntaxNode?[node.SlotCount];
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return index == 0 ? GetRed(ref _children[index]) : GetRed(ref _children[index], index);
    }

    internal override SyntaxNode? GetCachedSlot(int index)
    {
        return _children[index];
    }
}
