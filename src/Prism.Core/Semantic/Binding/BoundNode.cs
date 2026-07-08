// @file BoundNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Syntax;

namespace Prism.Core.Semantic.Binding;

public abstract class BoundNode
{
    public SyntaxNode Syntax { get; }

    public SemanticModel SemanticModel { get; }

    private protected BoundNode(SyntaxNode syntax, SemanticModel semanticModel)
    {
        Syntax = syntax;
        SemanticModel = semanticModel;
    }
}
