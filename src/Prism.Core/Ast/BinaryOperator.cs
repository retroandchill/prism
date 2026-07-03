// @file BinaryOperator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Ast;

public enum BinaryOperator
{
    Add,
    Sub,
    Mul,
    Div,
    Mod,

    LogicalAnd,
    LogicalOr,

    BitAnd,
    BitOr,
    BitXor,

    Equal,
    NotEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    ThreeWay,

    ShiftLeft,
    ShiftRight,
    UnsignedShiftRight,

    Assign,
    AddAssign,
    SubAssign,
    MulAssign,
    DivAssign,
    ModAssign,
    BitAndAssign,
    BitOrAssign,
    BitXorAssign,
    ShiftLeftAssign,
    ShiftRightAssign,
    UnsignedShiftRightAssign,
}
