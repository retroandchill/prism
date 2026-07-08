// @file SyntaxToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Syntax;

public readonly struct SyntaxToken
{
    public SyntaxNode? Parent { get; }
    internal GreenToken Green { get; }
    internal int Position { get; }

    public SyntaxKind Kind => Green.Kind;
    public int FullWidth => Green.FullWidth;
    public int Width => Green.Width;
    public string? Text => Green.Text;

    public TextSpan Range => new(Position, Width);

    public bool IsSynthetic => Green.IsMissing;
    public bool IsUnterminated => Green.IsUnterminatedLiteral;

    internal SyntaxToken(SyntaxNode parent, GreenToken green, int position)
    {
        Parent = parent;
        Green = green;
        Position = position;
    }

    internal SyntaxToken(GreenToken green, int position)
    {
        Green = green;
        Position = position;
    }
}
