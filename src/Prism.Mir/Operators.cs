// @file Operators.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Mir;

public enum UnaryOperator : byte
{
    Identity,
    Negation,
    LogicalNot,
    BitwiseNot,
}

public enum BinaryOperator : byte
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LogicalAnd,
    LogicalOr,
    Equality,
    NotEquals,
    LessThan,
    LessThanOrEquals,
    GreaterThan,
    GreaterThanOrEquals,
    ThreeWayComparison,
    ShiftLeft,
    ShiftRight,
    UnsignedShiftRight,
};
