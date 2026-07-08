// @file GreenNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax.Green;

internal abstract class GreenNode
{
    public SyntaxKind Kind { get; }

    public int FullWidth { get; }

    public int Width => FullWidth - LeadingTriviaWidth - TrailingTriviaWidth;

    public SyntaxFlags Flags { get; init; } = SyntaxFlags.None;

    public bool IsMissing => Flags.HasFlag(SyntaxFlags.Missing);

    public bool IsUnterminatedLiteral => Flags.HasFlag(SyntaxFlags.UnterminatedLiteral);

    public virtual int LeadingTriviaWidth => 0;

    public virtual int TrailingTriviaWidth => 0;

    protected GreenNode(SyntaxKind kind, int fullWidth)
    {
        Kind = kind;
        FullWidth = fullWidth;
    }
}
