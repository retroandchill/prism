using Prism.Syntax.Green;

namespace Prism.Syntax;

public closed class StructuredTriviaSyntax : SyntaxNode
{
    public SyntaxTrivia? ParentTrivia { get; private set; }

    private protected StructuredTriviaSyntax(GreenNode node, SyntaxNode? parent, int position)
        : base(node, parent, position) { }

    public static StructuredTriviaSyntax Create(in SyntaxTrivia trivia)
    {
        var parent = trivia.Parent.Parent;
        var red = (StructuredTriviaSyntax)trivia.Green.CreateRed(parent, trivia.Position);
        red.ParentTrivia = trivia;
        return red;
    }
}
