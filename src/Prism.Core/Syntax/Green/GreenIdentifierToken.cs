namespace Prism.Core.Syntax.Green;

internal sealed class GreenIdentifierToken(
    SyntaxKind kind,
    IdentifierInfo identifierInfo,
    GreenTriviaList? leadingTrivia = null,
    GreenTriviaList? trailingTrivia = null
) : GreenToken(kind, identifierInfo.Length, leadingTrivia, trailingTrivia)
{
    public IdentifierInfo IdentifierInfo { get; } = identifierInfo;

    public override string Text { get; } = identifierInfo.ToString();

    public override GreenIdentifierToken WithLeadingTrivia(GreenTriviaList? leadingTrivia)
    {
        return leadingTrivia == LeadingTrivia
            ? this
            : new GreenIdentifierToken(Kind, IdentifierInfo, leadingTrivia, TrailingTrivia);
    }

    public override GreenIdentifierToken WithTrailingTrivia(GreenTriviaList? trailingTrivia)
    {
        return trailingTrivia == TrailingTrivia
            ? this
            : new GreenIdentifierToken(Kind, IdentifierInfo, LeadingTrivia, trailingTrivia);
    }

    public override GreenIdentifierToken WithLeadingAndTrailingTrivia(
        GreenTriviaList leadingTrivia,
        GreenTriviaList trailingTrivia
    )
    {
        return leadingTrivia == LeadingTrivia && trailingTrivia == TrailingTrivia
            ? this
            : new GreenIdentifierToken(Kind, IdentifierInfo, leadingTrivia, trailingTrivia);
    }
}
