// @file SyntaxNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;

namespace Prism.Core.Syntax;

public abstract record SyntaxNode
{
    public required TextSpan Range { get; init; }

    public SyntaxFlags Flags { get; init; }
}
