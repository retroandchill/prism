// @file BoundExpression.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Utils;

namespace Prism.Core.Semantic.Binding;

public closed class BoundExpression : BoundNode
{
    public required TypeSymbol Type { get; init; }
}

public union BoundIntegerLiteralSyntax(IntegerLiteralExpressionSyntax, UnaryExpressionSyntax);
public union BoundFloatLiteralSyntax(FloatLiteralExpressionSyntax, UnaryExpressionSyntax);

public sealed class BoundBoolLiteralExpression : BoundExpression
{
    public required bool Value { get; init; }
    public required BooleanLiteralExpressionSyntax Syntax { get; init; }
}

public sealed class BoundIntegerLiteralExpression : BoundExpression
{
    public required BigInteger Value { get; init; }
    public required IntegerSuffix Suffix { get; init; }
    public required BoundIntegerLiteralSyntax Syntax { get; init; }
}

public sealed class BoundFloatLiteralExpression : BoundExpression
{
    public required decimal Value { get; init; }
    public required FloatSuffix Suffix { get; init; }
    public required BoundFloatLiteralSyntax Syntax { get; init; }
}

public sealed class BoundStringLiteralExpression : BoundExpression
{
    public required string Value { get; init; }
    public required StringLiteralExpressionSyntax Syntax { get; init; }
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

    public required BoundExpression Left { get; init; }

    public required BoundExpression Right { get; init; }

    public required BinaryOperator Operator { get; init; }

    public required TypeSymbol LeftType { get; init; }

    public required TypeSymbol RightType { get; init; }
}

public sealed class BoundTernaryExpression : BoundExpression
{
    public required TernaryExpressionSyntax Syntax { get; init; }

    public required BoundExpression Condition { get; init; }

    public required BoundExpression TrueExpression { get; init; }

    public required BoundExpression FalseExpression { get; init; }
}

public sealed class BoundInvocationExpression : BoundExpression
{
    public required InvocationExpressionSyntax Syntax { get; init; }

    public required FunctionSymbol CalledFunction { get; init; }

    public required ImmutableArray<BoundExpression> Arguments { get; init; }
}
