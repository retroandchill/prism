// @file GreenToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal class GreenToken : GreenNode
{
    public virtual string Text => Kind.GetTokenText();

    public GreenTriviaList LeadingTrivia { get; }
    public sealed override int LeadingTriviaWidth => LeadingTrivia.FullWidth;
    public GreenTriviaList TrailingTrivia { get; }
    public sealed override int TrailingTriviaWidth => TrailingTrivia.FullWidth;

    public GreenToken(
        SyntaxKind kind,
        GreenTriviaList? leadingTrivia = null,
        GreenTriviaList? trailingTrivia = null
    )
        : this(kind, kind.GetTokenText().Length, leadingTrivia, trailingTrivia) { }

    public GreenToken(
        SyntaxKind kind,
        int codeWidth,
        GreenTriviaList? leadingTrivia = null,
        GreenTriviaList? trailingTrivia = null
    )
        : base(kind, codeWidth + (leadingTrivia?.FullWidth ?? 0) + (trailingTrivia?.FullWidth ?? 0))
    {
        LeadingTrivia = leadingTrivia ?? GreenTriviaList.Empty;
        TrailingTrivia = trailingTrivia ?? GreenTriviaList.Empty;
        ChildCount = (!LeadingTrivia.IsEmpty ? 1 : 0) + (!TrailingTrivia.IsEmpty ? 1 : 0);
    }

    public override GreenTriviaList? GetChild(int index)
    {
        switch (index)
        {
            case 0:
                if (!LeadingTrivia.IsEmpty)
                    return LeadingTrivia;
                if (!TrailingTrivia.IsEmpty)
                    return TrailingTrivia;
                break;
            case 1 when !LeadingTrivia.IsEmpty:
                return !TrailingTrivia.IsEmpty ? TrailingTrivia : null;
        }

        return null;
    }

    public virtual GreenToken WithLeadingTrivia(GreenTriviaList leadingTrivia)
    {
        return leadingTrivia == LeadingTrivia
            ? this
            : new GreenToken(Kind, Width, leadingTrivia, TrailingTrivia);
    }

    public virtual GreenToken WithTrailingTrivia(GreenTriviaList trailingTrivia)
    {
        return trailingTrivia == TrailingTrivia
            ? this
            : new GreenToken(Kind, Width, LeadingTrivia, trailingTrivia);
    }

    public virtual GreenToken WithLeadingAndTrailingTrivia(
        GreenTriviaList leadingTrivia,
        GreenTriviaList trailingTrivia
    )
    {
        return leadingTrivia == LeadingTrivia && trailingTrivia == TrailingTrivia
            ? this
            : new GreenToken(Kind, Width, leadingTrivia, trailingTrivia);
    }
}
