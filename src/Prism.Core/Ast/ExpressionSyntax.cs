// @file ExpressionSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Strings;
#pragma warning disable CA1067

namespace Prism.Core.Ast;

public closed record ExpressionSyntax : SyntaxNode;

public enum LiteralKind
{
    BoolTrue,
    BoolFalse,
    Integer,
    Float,
    String
}

public sealed record LiteralExpressionSyntax : ExpressionSyntax
{
    public required LiteralKind Kind { get; init; }
}

public sealed record IdentifierExpressionSyntax : ExpressionSyntax
{
    public required Name Name { get; init; }
}

public sealed record BinaryExpressionSyntax : ExpressionSyntax
{
    public required ExpressionSyntax Left { get; init; }
    public required BinaryOperator Operator { get; init; }
    public required ExpressionSyntax Right { get; init; }
}

public sealed record UnaryExpressionSyntax : ExpressionSyntax
{
    public required UnaryOperator Operator { get; init; }
    public required ExpressionSyntax Operand { get; init; }
}

public sealed record TernaryExpressionSyntax : ExpressionSyntax
{
    public required ExpressionSyntax Condition { get; init; }
    public required ExpressionSyntax TrueExpression { get; init; }
    public required ExpressionSyntax FalseExpression { get; init; }
}

public sealed record InvocationExpressionSyntax : ExpressionSyntax
{
    public required ExpressionSyntax Callee { get; init; }
    public ImmutableArray<ExpressionSyntax> Arguments { get; init; } = [];
}
