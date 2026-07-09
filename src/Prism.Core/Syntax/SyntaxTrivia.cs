// @file SyntaxTrivia.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public readonly struct SyntaxTrivia : ISyntaxElement
{
    public SyntaxToken Parent { get; }
    internal GreenTrivia Green { get; }
    internal int Position { get; }

    public SyntaxKind Kind => Green.Kind;
    public TextSpan FullSpan => new(Position, Green.FullWidth);

    public TextSpan Span => new(Position, Green.Width);

    public bool ContainsDiagnostics => Green.ContainsDiagnostics;

    internal SyntaxTrivia(SyntaxToken parent, GreenTrivia green, int position)
    {
        Parent = parent;
        Green = green;
        Position = position;
    }
}
