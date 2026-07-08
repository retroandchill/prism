// @file GreenTriviaList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal sealed class GreenTriviaList(GreenTrivia[] trivia)
    : GreenNode(SyntaxKind.TriviaList, CalculateWidth(trivia))
{
    private readonly GreenTrivia[] _trivias;

    public static readonly GreenTriviaList Empty = new([]);

    private static int CalculateWidth(GreenTrivia[] trivia)
    {
        return trivia.Sum(t => t.FullWidth);
    }
}
