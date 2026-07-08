// @file GreenTrivia.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal sealed class GreenTrivia(SyntaxKind kind, string text) : GreenNode(kind, text.Length)
{
    public static readonly GreenTrivia CarriageReturnLineFeed = new(
        SyntaxKind.NewLineTrivia,
        "\r\n"
    );
    public static readonly GreenTrivia LineFeed = new(SyntaxKind.NewLineTrivia, "\n");
    public static readonly GreenTrivia CarriageReturn = new(SyntaxKind.NewLineTrivia, "\r");

    public string Text { get; } = text;
}
