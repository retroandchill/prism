// @file BinaryOperator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using NetEscapades.EnumGenerators;
using Prism.Core.Parse;

namespace Prism.Core.Ast;

[EnumExtensions(ExtensionClassName = "BinaryOperators")]
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

    NullCoalesce,

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

public static partial class BinaryOperators
{
    public static BinaryOperator ToBinaryOperator(this TokenKind kind)
    {
        return kind switch
        {
            TokenKind.Amp => BinaryOperator.BitAnd,
            TokenKind.AmpAmp => BinaryOperator.LogicalAnd,
            TokenKind.AmpEqual => BinaryOperator.BitAndAssign,
            TokenKind.Star => BinaryOperator.Mul,
            TokenKind.StarEqual => BinaryOperator.MulAssign,
            TokenKind.Plus => BinaryOperator.Add,
            TokenKind.PlusEqual => BinaryOperator.AddAssign,
            TokenKind.Minus => BinaryOperator.Sub,
            TokenKind.MinusEqual => BinaryOperator.SubAssign,
            TokenKind.ExclaimEqual => BinaryOperator.NotEqual,
            TokenKind.Slash => BinaryOperator.Div,
            TokenKind.SlashEqual => BinaryOperator.DivAssign,
            TokenKind.Percent => BinaryOperator.Mod,
            TokenKind.PercentEqual => BinaryOperator.ModAssign,
            TokenKind.Less => BinaryOperator.Less,
            TokenKind.LessLess => BinaryOperator.ShiftLeft,
            TokenKind.LessEqual => BinaryOperator.LessEqual,
            TokenKind.LessLessEqual => BinaryOperator.ShiftLeftAssign,
            TokenKind.Spaceship => BinaryOperator.ThreeWay,
            TokenKind.Greater => BinaryOperator.Greater,
            TokenKind.GreaterGreater => BinaryOperator.ShiftRight,
            TokenKind.GreaterGreaterGreater => BinaryOperator.UnsignedShiftRight,
            TokenKind.GreaterEqual => BinaryOperator.GreaterEqual,
            TokenKind.GreaterGreaterEqual => BinaryOperator.ShiftRightAssign,
            TokenKind.GreaterGreaterGreaterEqual => BinaryOperator.UnsignedShiftRightAssign,
            TokenKind.Caret => BinaryOperator.BitXor,
            TokenKind.CaretEqual => BinaryOperator.BitXorAssign,
            TokenKind.Pipe => BinaryOperator.BitOr,
            TokenKind.PipePipe => BinaryOperator.LogicalOr,
            TokenKind.PipeEqual => BinaryOperator.BitOrAssign,
            TokenKind.QuestionQuestion => BinaryOperator.NullCoalesce,
            TokenKind.Equal => BinaryOperator.Assign,
            TokenKind.EqualEqual => BinaryOperator.Equal,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    extension(BinaryOperator op)
    {
        public bool IsAssignmentOperator => op.AsUnderlyingType() >= BinaryOperator.Assign.AsUnderlyingType();
    }
}
