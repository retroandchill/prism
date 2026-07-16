// @file SyntaxTrivia.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxTrivia
{
    public string Name { get; }

    public string? DisplayName { get; }

    public SyntaxKind Kind { get; internal set; } = null!;

    internal SyntaxTrivia(string name, string? displayName = null)
    {
        Name = name;
        DisplayName = displayName;
    }
}
