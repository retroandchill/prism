// @file ExpressionSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using Prism.Core.Strings;
#pragma warning disable CA1067

namespace Prism.Core.Ast;

public closed record ExpressionSyntax : SyntaxNode;

public sealed record BooleanLiteralExpressionSyntax : ExpressionSyntax
{
    public required bool Value { get; init; }
}

public enum IntegerBase : byte
{
    Decimal,
    Hex,
    Binary
}

public enum IntegerSuffix : byte
{
    None,
    I8,
    I16,
    I32,
    I64,
    I128,
    ISize,
    U8,
    U16,
    U32,
    U64,
    U128,
    USize
}

public sealed record IntegerLiteralExpressionSyntax : ExpressionSyntax
{
    public required IntegerBase Base { get; init; }

    public required BigInteger Value { get; init; }

    public required IntegerSuffix Suffix { get; init; }
}

public enum FloatSuffix : byte
{
    None,
    F16,
    F32,
    F64
}

public sealed record FloatLiteralExpressionSyntax : ExpressionSyntax
{
    public required decimal Value { get; init; }

    public required FloatSuffix Suffix { get; init; }

}

public sealed record StringLiteralExpressionSyntax : ExpressionSyntax
{
    public required string Value { get; init; }
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

public sealed record NamedArgumentSyntax : ExpressionSyntax
{
    public required IdentifierSyntax Identifier { get; init; }
    public required ExpressionSyntax Value { get; init; }
}

public sealed record InvalidExpressionSyntax : ExpressionSyntax;
