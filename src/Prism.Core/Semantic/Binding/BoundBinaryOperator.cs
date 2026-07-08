// @file BoundBinaryOperator.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Riok.Mapperly.Abstractions;

namespace Prism.Core.Semantic.Binding;

public enum BoundBinaryOperator
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
}

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class BinaryOperatorMapper
{
    [MapEnumValue(BinaryOperator.AddAssign, BoundBinaryOperator.Add)]
    [MapEnumValue(BinaryOperator.SubAssign, BoundBinaryOperator.Sub)]
    [MapEnumValue(BinaryOperator.MulAssign, BoundBinaryOperator.Mul)]
    [MapEnumValue(BinaryOperator.DivAssign, BoundBinaryOperator.Div)]
    [MapEnumValue(BinaryOperator.ModAssign, BoundBinaryOperator.Mod)]
    [MapEnumValue(BinaryOperator.BitAndAssign, BoundBinaryOperator.BitAnd)]
    [MapEnumValue(BinaryOperator.BitOrAssign, BoundBinaryOperator.BitOr)]
    [MapEnumValue(BinaryOperator.BitXorAssign, BoundBinaryOperator.BitXor)]
    [MapEnumValue(BinaryOperator.ShiftLeftAssign, BoundBinaryOperator.ShiftLeft)]
    [MapEnumValue(BinaryOperator.ShiftRightAssign, BoundBinaryOperator.ShiftRight)]
    [MapEnumValue(BinaryOperator.UnsignedShiftRightAssign, BoundBinaryOperator.UnsignedShiftRight)]
    [MapperIgnoreSourceValue(BinaryOperator.Assign)]
    public static partial BoundBinaryOperator ToBoundOperator(this BinaryOperator kind);
}