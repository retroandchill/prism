// @file GreenTrivia.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.Core.Syntax.Green;

internal sealed class GreenTrivia(SyntaxKind kind, string text) : GreenNode(kind, text.Length)
{
    public static readonly GreenTrivia CarriageReturnLineFeed = new(
        SyntaxKind.NewLineTrivia,
        "\r\n"
    );
    public static readonly GreenTrivia LineFeed = new(SyntaxKind.NewLineTrivia, "\n");
    public static readonly GreenTrivia CarriageReturn = new(SyntaxKind.NewLineTrivia, "\r");

    public override int Width => Text.Length;

    public override GreenNode? LeadingTrivia => null;
    public override int LeadingTriviaWidth => 0;
    public override GreenNode? TrailingTrivia => null;
    public override int TrailingTriviaWidth => 0;

    public string Text { get; } = text;

    public override GreenNode? GetSlot(int index) => null;

    public override GreenTrivia WithDiagnostics(ImmutableArray<SyntaxDiagnosticInfo> diagnostics)
    {
        if (diagnostics == Diagnostics)
            return this;

        return new GreenTrivia(Kind, Text) { Diagnostics = diagnostics };
    }

    public override SyntaxNode CreateRed(SyntaxNode? parent = null, int positon = 0)
    {
        throw new InvalidOperationException("Cannot create a red node from a Green Trivia");
    }
}
