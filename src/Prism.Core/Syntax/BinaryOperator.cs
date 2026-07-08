// @file BinaryOperator.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using NetEscapades.EnumGenerators;

namespace Prism.Core.Syntax;

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
    public static BinaryOperator ToBinaryOperator(this SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.AmpToken => BinaryOperator.BitAnd,
            SyntaxKind.AmpAmpToken => BinaryOperator.LogicalAnd,
            SyntaxKind.AmpEqualToken => BinaryOperator.BitAndAssign,
            SyntaxKind.StarToken => BinaryOperator.Mul,
            SyntaxKind.StarEqualToken => BinaryOperator.MulAssign,
            SyntaxKind.PlusToken => BinaryOperator.Add,
            SyntaxKind.PlusEqualToken => BinaryOperator.AddAssign,
            SyntaxKind.MinusToken => BinaryOperator.Sub,
            SyntaxKind.MinusEqualToken => BinaryOperator.SubAssign,
            SyntaxKind.ExclaimEqualToken => BinaryOperator.NotEqual,
            SyntaxKind.SlashToken => BinaryOperator.Div,
            SyntaxKind.SlashEqualToken => BinaryOperator.DivAssign,
            SyntaxKind.PercentToken => BinaryOperator.Mod,
            SyntaxKind.PercentEqualToken => BinaryOperator.ModAssign,
            SyntaxKind.LessToken => BinaryOperator.Less,
            SyntaxKind.LessLessToken => BinaryOperator.ShiftLeft,
            SyntaxKind.LessEqualToken => BinaryOperator.LessEqual,
            SyntaxKind.LessLessEqualToken => BinaryOperator.ShiftLeftAssign,
            SyntaxKind.SpaceshipToken => BinaryOperator.ThreeWay,
            SyntaxKind.GreaterToken => BinaryOperator.Greater,
            SyntaxKind.GreaterGreaterToken => BinaryOperator.ShiftRight,
            SyntaxKind.GreaterGreaterGreaterToken => BinaryOperator.UnsignedShiftRight,
            SyntaxKind.GreaterEqualToken => BinaryOperator.GreaterEqual,
            SyntaxKind.GreaterGreaterEqualToken => BinaryOperator.ShiftRightAssign,
            SyntaxKind.GreaterGreaterGreaterEqualToken => BinaryOperator.UnsignedShiftRightAssign,
            SyntaxKind.CaretToken => BinaryOperator.BitXor,
            SyntaxKind.CaretEqualToken => BinaryOperator.BitXorAssign,
            SyntaxKind.PipeToken => BinaryOperator.BitOr,
            SyntaxKind.PipePipeToken => BinaryOperator.LogicalOr,
            SyntaxKind.PipeEqualToken => BinaryOperator.BitOrAssign,
            SyntaxKind.QuestionQuestionToken => BinaryOperator.NullCoalesce,
            SyntaxKind.EqualToken => BinaryOperator.Assign,
            SyntaxKind.EqualEqualToken => BinaryOperator.Equal,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    extension(BinaryOperator op)
    {
        public bool IsAssignmentOperator =>
            op.AsUnderlyingType() >= BinaryOperator.Assign.AsUnderlyingType();
    }
}
