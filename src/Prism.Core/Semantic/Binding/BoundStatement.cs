// @file BoundStatement.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic.Binding;

public abstract class BoundStatement : BoundNode
{
    private protected BoundStatement(SyntaxNode syntax, SemanticModel semanticModel)
        : base(syntax, semanticModel) { }
}

public sealed class BoundBlock : BoundStatement
{
    public ImmutableArray<BoundStatement> Statements { get; }

    internal BoundBlock(
        SyntaxNode syntax,
        SemanticModel semanticModel,
        ImmutableArray<BoundStatement> statements
    )
        : base(syntax, semanticModel)
    {
        Statements = statements;
    }
}

public sealed class BoundLocalVariable : BoundStatement
{
    public VariableSymbol Symbol { get; }

    public BoundExpression? Initializer { get; }

    internal BoundLocalVariable(
        SyntaxNode syntax,
        SemanticModel semanticModel,
        VariableSymbol symbol,
        BoundExpression? initializer
    )
        : base(syntax, semanticModel)
    {
        Symbol = symbol;
        Initializer = initializer;
    }
}

public sealed class BoundExpressionStatement : BoundStatement
{
    public BoundExpression Expression { get; }

    internal BoundExpressionStatement(
        SyntaxNode syntax,
        SemanticModel semanticModel,
        BoundExpression expression
    )
        : base(syntax, semanticModel)
    {
        Expression = expression;
    }
}

public sealed class BoundReturnStatement : BoundStatement
{
    public BoundExpression? Expression { get; }

    internal BoundReturnStatement(
        SyntaxNode syntax,
        SemanticModel semanticModel,
        BoundExpression? expression
    )
        : base(syntax, semanticModel)
    {
        Expression = expression;
    }
}
