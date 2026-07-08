// @file Operators.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Semantic;
using Prism.Core.Semantic.Binding;
using Prism.Core.Syntax;

namespace Prism.Core.Utils;

internal readonly record struct BinaryOperatorInfo(
    BuiltInType LeftType,
    BuiltInType RightType,
    BuiltInType ResultType
);

internal static class Operators
{
    public static bool TryResolveUnary(
        UnaryOperator @operator,
        BuiltInType type,
        out BuiltInType resultType
    )
    {
        switch (@operator)
        {
            case UnaryOperator.LogicalNot when type is BuiltInType.Bool:
                resultType = type;
                return true;
            case UnaryOperator.Negate when type.IsNumeric && type is not BuiltInType.U128:
            case UnaryOperator.BitNot when type.IsInteger:
            case UnaryOperator.Positive when type.IsNumeric:
                resultType = ApplyArithmeticPromotion(type);
                return true;
            case UnaryOperator.PreIncrement
            or UnaryOperator.PostIncrement
            or UnaryOperator.PreDecrement
            or UnaryOperator.PostDecrement when type.IsNumeric:
                resultType = type;
                return true;
        }

        resultType = default;
        return false;
    }

    private static BuiltInType ApplyArithmeticPromotion(BuiltInType type)
    {
        return type switch
        {
            BuiltInType.I8 or BuiltInType.I16 or BuiltInType.U8 or BuiltInType.U16 =>
                BuiltInType.I32,
            BuiltInType.F16 => BuiltInType.F32,
            _ => type,
        };
    }

    public static bool TryResolveBinary(
        BoundBinaryOperator @operator,
        BuiltInType leftType,
        BuiltInType rightType,
        TargetPlatform platform,
        out BinaryOperatorInfo result
    )
    {
        switch (@operator)
        {
            case BoundBinaryOperator.Add
            or BoundBinaryOperator.Sub
            or BoundBinaryOperator.Mul
            or BoundBinaryOperator.Div
            or BoundBinaryOperator.Mod when leftType.IsNumeric && rightType.IsNumeric:
            case BoundBinaryOperator.BitAnd
            or BoundBinaryOperator.BitOr
            or BoundBinaryOperator.BitXor when leftType.IsInteger && rightType.IsInteger:
            {
                if (TryGetCommonNumericType(leftType, rightType, platform, out var common))
                {
                    result = new BinaryOperatorInfo(common, common, common);
                    return true;
                }

                break;
            }
            case BoundBinaryOperator.Equal or BoundBinaryOperator.NotEqual:
            {
                if (
                    leftType.IsNumeric
                    && rightType.IsNumeric
                    && TryGetCommonNumericType(leftType, rightType, platform, out var common)
                )
                {
                    result = new BinaryOperatorInfo(common, common, BuiltInType.Bool);
                    return true;
                }

                if (leftType == rightType)
                {
                    result = new BinaryOperatorInfo(leftType, rightType, BuiltInType.Bool);
                    return true;
                }

                if (leftType.IsCharacter && rightType.IsCharacter)
                {
                    common = GetCommonChar(leftType, rightType);
                    result = new BinaryOperatorInfo(common, common, BuiltInType.Bool);
                    return true;
                }

                break;
            }
            case BoundBinaryOperator.Greater
            or BoundBinaryOperator.GreaterEqual
            or BoundBinaryOperator.Less
            or BoundBinaryOperator.LessEqual when leftType.IsNumeric && rightType.IsNumeric:
            {
                if (TryGetCommonNumericType(leftType, rightType, platform, out var common))
                {
                    result = new BinaryOperatorInfo(common, common, BuiltInType.Bool);
                    return true;
                }

                break;
            }
            case BoundBinaryOperator.LogicalAnd
            or BoundBinaryOperator.LogicalOr
                when leftType is BuiltInType.Bool && rightType is BuiltInType.Bool:
                result = new BinaryOperatorInfo(
                    BuiltInType.Bool,
                    BuiltInType.Bool,
                    BuiltInType.Bool
                );
                return true;
            case BoundBinaryOperator.ShiftLeft
            or BoundBinaryOperator.ShiftRight
            or BoundBinaryOperator.UnsignedShiftRight
                when leftType.IsInteger && rightType.IsInteger:
            {
                var promoted = ApplyArithmeticPromotion(leftType);
                result = new BinaryOperatorInfo(promoted, rightType, promoted);
                return true;
            }
        }

        result = default;
        return false;
    }

    private static bool TryGetCommonNumericType(
        BuiltInType leftType,
        BuiltInType rightType,
        TargetPlatform platform,
        out BuiltInType common
    )
    {
        var promotedLeft = ApplyArithmeticPromotion(leftType);
        var promotedRight = ApplyArithmeticPromotion(rightType);

        if (promotedLeft == promotedRight)
        {
            common = promotedLeft;
            return true;
        }

        if (leftType.IsFloatingPoint || rightType.IsFloatingPoint)
        {
            if (leftType.IsFloatingPoint && rightType.IsFloatingPoint)
            {
                common =
                    leftType.GetByteWidth(platform) > rightType.GetByteWidth(platform)
                        ? leftType
                        : rightType;
                return true;
            }

            var integer = leftType.IsInteger ? leftType : rightType;

            if (integer.GetByteWidth(platform) > 4)
            {
                common = default;
                return false;
            }

            common = integer is BuiltInType.I32 or BuiltInType.U32
                ? BuiltInType.F64
                : BuiltInType.F32;
            return true;
        }

        if (leftType.IsUnsigned == rightType.IsUnsigned)
        {
            common =
                leftType.GetByteWidth(platform) >= rightType.GetByteWidth(platform)
                    ? leftType
                    : rightType;
            return true;
        }

        var unsignedType = leftType.IsUnsigned ? leftType : rightType;
        var signedType = leftType.IsSigned ? leftType : rightType;

        if (signedType is BuiltInType.I128 && unsignedType is BuiltInType.U128)
        {
            common = default;
            return false;
        }

        var unsignedWidth = unsignedType.GetByteWidth(platform);
        var signedWidth = signedType.GetByteWidth(platform);

        if (signedWidth > unsignedWidth)
        {
            common = signedType;
        }

        common = unsignedType switch
        {
            BuiltInType.U8 or BuiltInType.U16 => BuiltInType.I32,
            BuiltInType.U32 => BuiltInType.I64,
            BuiltInType.U64 => BuiltInType.I128,
            _ => throw new InvalidOperationException(),
        };
        return true;
    }

    private static BuiltInType GetCommonChar(BuiltInType left, BuiltInType right)
    {
        return left.AsUnderlyingType() > right.AsUnderlyingType() ? left : right;
    }
}
