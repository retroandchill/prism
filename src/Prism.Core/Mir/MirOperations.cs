// @file MirOperations.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir;

internal enum MirUnaryOp : byte
{
    Negation,
    LogicalNot,
    BitwiseNot,
}

internal enum MirBinaryOp : byte
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    ShiftLeft,
    ShiftRight,
    UnsignedShiftRight,
    LogicalAnd,
    LogicalOr,
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
}

internal enum MirConversionKind : byte
{
    Identity,
    ZeroExtend,
    SignExtend,
    Truncate,
    FloatExtend,
    FloatTruncate,
    SignedIntToFloat,
    UnsignedIntToFloat,
    FloatToSignedInt,
    FloatToUnsignedInt,
    BitCast,
}
