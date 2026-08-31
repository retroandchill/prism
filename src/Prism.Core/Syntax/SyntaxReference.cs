using Prism.Core.Text;

namespace Prism.Core.Syntax;

public readonly struct SyntaxReference(SyntaxNode syntax)
{
    public SyntaxNode Syntax { get; } = syntax;

    public SyntaxTree SyntaxTree => Syntax.SyntaxTree;

    public TextSpan SourceSpan => Syntax.Span;
}
