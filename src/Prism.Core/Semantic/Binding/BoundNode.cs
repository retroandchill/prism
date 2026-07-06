// @file BoundNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;

namespace Prism.Core.Semantic.Binding;

public closed class BoundNode
{
    public required SyntaxNode Syntax { get; init; }
}
