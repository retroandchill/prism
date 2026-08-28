// @file SyntaxToken.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public enum TokenCategory
{
    Keyword,
    Punctuation,
    Other,
}

[Flags]
public enum TokenFlags
{
    None = 0,
    Contextual = 1 << 0,
}

public sealed class SyntaxToken : ISyntaxVariant
{
    public string Name { get; }

    public TokenCategory Category { get; }

    public SyntaxKind Kind { get; internal set; } = null!;

    public TokenFlags Flags { get; internal init; }

    public bool Contextual => Flags.HasFlag(TokenFlags.Contextual);

    public string? Text { get; internal init; }

    public string? DisplayName { get; internal init; }

    internal SyntaxToken(string name, TokenCategory category)
    {
        Name = name;
        Category = category;
    }
}
