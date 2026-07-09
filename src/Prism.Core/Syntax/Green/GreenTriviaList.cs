// @file GreenTriviaList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal sealed class GreenTriviaList : GreenNode
{
    private readonly GreenTrivia[] _trivia;

    public static readonly GreenTriviaList Empty = new([]);

    public bool IsEmpty => _trivia.Length == 0;

    public GreenTriviaList(GreenTrivia[] trivia)
        : base(SyntaxKind.TriviaList, CalculateWidth(trivia))
    {
        _trivia = trivia;
        ChildCount = _trivia.Length;
    }

    private static int CalculateWidth(GreenTrivia[] trivia)
    {
        return trivia.Sum(t => t.FullWidth);
    }

    public override GreenTrivia? GetChild(int index)
    {
        return index < _trivia.Length ? _trivia[index] : null;
    }
}
