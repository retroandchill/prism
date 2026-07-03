// @file SyntaxNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;

namespace Prism.Core.Ast;

[Flags]
public enum SyntaxFlags
{
    None = 0,
    Missing = 1 << 0,
    Unknown = 1 << 1
}

public closed record SyntaxNode
{
    public required SourceRange Range { get; init; }

    public SyntaxFlags Flags { get; init; }
}
