namespace Prism.Core.Syntax.Green;

internal static class GreenLiteralToken
{
    public static GreenLiteralToken<BoolLiteral> BoolTrue => Create(BoolLiteral.True, "true");
    public static GreenLiteralToken<BoolLiteral> BoolFalse => Create(BoolLiteral.False, "false");

    public static GreenLiteralToken<T> Create<T>(
        T value,
        string rawText,
        GreenTriviaList? leadingTrivia = null,
        GreenTriviaList? trailingTrivia = null
    )
        where T : ISyntaxLiteral
    {
        return new GreenLiteralToken<T>(value, rawText, leadingTrivia, trailingTrivia);
    }
}

internal sealed class GreenLiteralToken<T>(
    T value,
    string rawText,
    GreenTriviaList? leadingTrivia = null,
    GreenTriviaList? trailingTrivia = null
) : GreenToken(value.Kind, rawText.Length, leadingTrivia, trailingTrivia)
    where T : ISyntaxLiteral
{
    public T Value { get; } = value;

    public override string Text { get; } = rawText;

    public override GreenLiteralToken<T> WithLeadingTrivia(GreenTriviaList? leadingTrivia)
    {
        return leadingTrivia == LeadingTrivia
            ? this
            : new GreenLiteralToken<T>(Value, Text, leadingTrivia, TrailingTrivia);
    }

    public override GreenLiteralToken<T> WithTrailingTrivia(GreenTriviaList? trailingTrivia)
    {
        return trailingTrivia == TrailingTrivia
            ? this
            : new GreenLiteralToken<T>(Value, Text, LeadingTrivia, trailingTrivia);
    }

    public override GreenLiteralToken<T> WithLeadingAndTrailingTrivia(
        GreenTriviaList leadingTrivia,
        GreenTriviaList trailingTrivia
    )
    {
        return leadingTrivia == LeadingTrivia && trailingTrivia == TrailingTrivia
            ? this
            : new GreenLiteralToken<T>(Value, Text, leadingTrivia, trailingTrivia);
    }
}
