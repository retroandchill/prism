using Prism.Text;

namespace Prism.Syntax;

public readonly struct SyntaxReference(SyntaxNode syntax)
{
    public SyntaxNode Syntax { get; } = syntax;

    public SyntaxTree SyntaxTree => Syntax.SyntaxTree;

    public TextSpan SourceSpan => Syntax.Span;
}
