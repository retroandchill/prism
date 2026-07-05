// @file BoundExpression.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Utils;

namespace Prism.Core.Semantic.Binding;

public closed class BoundExpression : BoundNode
{
    public required TypeSymbol Type { get; init; }
}

public sealed class BoundLiteralExpression : BoundExpression
{
    public required LiteralValue Value { get; init; }
}

public sealed class BoundVariableExpression : BoundExpression
{
    public required IdentifierExpressionSyntax Syntax { get; init; }

    public required VariableSymbol Symbol { get; init; }
}

public sealed class BoundUnaryExpression : BoundExpression
{
    public required UnaryExpressionSyntax Syntax { get; init; }

    public required BoundExpression Operand { get; init; }

    public required UnaryOperator Operator { get; init; }
}

public sealed class BoundBinaryExpression : BoundExpression
{
    public required BinaryExpressionSyntax Syntax { get; init; }

    public required BoundExpression Operand { get; init; }

    public required UnaryOperator Operator { get; init; }
}

public sealed class BoundTernaryExpression : BoundExpression
{
    public required TernaryExpressionSyntax Syntax { get; init; }

    public required BoundExpression Condition { get; init; }

    public required BoundExpression TrueExpression { get; init; }

    public required BoundExpression FalseExpression { get; init; }
}
