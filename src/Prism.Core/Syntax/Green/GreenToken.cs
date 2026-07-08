// @file GreenToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Strings;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenToken : GreenNode
{
    public string? Text { get; private init; }

    public Name IdentifierName { get; private init; }

    public GreenTriviaList LeadingTrivia { get; }
    public override int LeadingTriviaWidth => LeadingTrivia.FullWidth;
    public GreenTriviaList TrailingTrivia { get; }
    public override int TrailingTriviaWidth => TrailingTrivia.FullWidth;

    public GreenToken(
        SyntaxKind kind,
        Name name,
        GreenTriviaList? leadingTrivia = null,
        GreenTriviaList? trailingTrivia = null
    )
        : this(kind, name.ToString(), leadingTrivia, trailingTrivia)
    {
        IdentifierName = name;
    }

    public GreenToken(
        SyntaxKind kind,
        string text,
        GreenTriviaList? leadingTrivia = null,
        GreenTriviaList? trailingTrivia = null
    )
        : this(kind, text.Length, leadingTrivia, trailingTrivia)
    {
        Text = text;
    }

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
    }

    public GreenToken WithLeadingTrivia(GreenTriviaList leadingTrivia)
    {
        return leadingTrivia == LeadingTrivia
            ? this
            : new GreenToken(Kind, Width, leadingTrivia, TrailingTrivia);
    }

    public GreenToken WithTrailingTrivia(GreenTriviaList trailingTrivia)
    {
        return trailingTrivia == TrailingTrivia
            ? this
            : new GreenToken(Kind, Width, LeadingTrivia, trailingTrivia);
    }

    public GreenToken WithLeadingAndTrailingTrivia(
        GreenTriviaList leadingTrivia,
        GreenTriviaList trailingTrivia
    )
    {
        return leadingTrivia == LeadingTrivia && trailingTrivia == TrailingTrivia
            ? this
            : new GreenToken(Kind, Width, leadingTrivia, trailingTrivia);
    }
}
