using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract class BoundNode(SyntaxNode syntax)
{
    public SyntaxNode Syntax { get; } = syntax;
}
