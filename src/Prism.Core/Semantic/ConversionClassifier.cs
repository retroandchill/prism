using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Compiling;
using Prism.Core.Configuration;
using Prism.Core.Symbols;

namespace Prism.Core.Semantic;

internal readonly record struct OperandConversion(Conversion Conversion, TypeSymbol Type);

internal readonly record struct BinaryOperandConversion(
    Conversion LeftConversion,
    Conversion RightConversion,
    TypeSymbol Type
);

internal sealed class ConversionClassifier(Binder binder)
{
    public Conversion ClassifyConversion(TypeSymbol source, TypeSymbol target)
    {
        if (source == target)
        {
            return Conversion.GetTrivial(ConversionKind.Identity);
        }

        if (IsNumericType(source) && IsNumericType(target))
        {
            return ClassifyNumericConversion(source.SpecialType, target.SpecialType);
        }

        if (IsCharacterType(source) && IsCharacterType(target))
        {
            return ClassifyCharacterConversion(source.SpecialType, target.SpecialType);
        }

        return Conversion.None;
    }

    public OperandConversion? ClassifyUnaryOperand(UnaryOperation operation, TypeSymbol operand)
    {
        switch (operation)
        {
            case UnaryOperation.Identity:
                if (IsNumericType(operand))
                {
                    var promoted = PromoteNumericType(operand);
                    return new OperandConversion(
                        ClassifyNumericConversion(operand.SpecialType, promoted.SpecialType),
                        promoted
                    );
                }
                break;
            case UnaryOperation.Negation:
                if (IsNumericType(operand) && PromoteNegationType(operand) is { } negated)
                {
                    return new OperandConversion(
                        ClassifyNumericConversion(operand.SpecialType, negated.SpecialType),
                        negated
                    );
                }
                break;
            case UnaryOperation.LogicalNot:
                if (operand.SpecialType == SpecialType.Bool)
                {
                    return new OperandConversion(
                        Conversion.GetTrivial(ConversionKind.Identity),
                        operand
                    );
                }
                break;
            case UnaryOperation.BitwiseNot:
                if (IsIntegralType(operand))
                {
                    var promoted = PromoteNumericType(operand);
                    return new OperandConversion(
                        ClassifyNumericConversion(operand.SpecialType, promoted.SpecialType),
                        promoted
                    );
                }
                break;
            case UnaryOperation.PreIncrement:
            case UnaryOperation.PreDecrement:
            case UnaryOperation.PostIncrement:
            case UnaryOperation.PostDecrement:
                if (IsNumericType(operand))
                {
                    return new OperandConversion(
                        Conversion.GetTrivial(ConversionKind.Identity),
                        operand
                    );
                }
                break;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }

        return null;
    }

    public BinaryOperandConversion? ClassifyBinaryOperand(
        BinaryOperation operation,
        TypeSymbol left,
        TypeSymbol right
    )
    {
        switch (operation)
        {
            case BinaryOperation.Add:
            case BinaryOperation.Subtract:
            case BinaryOperation.Multiply:
            case BinaryOperation.Divide:
            case BinaryOperation.Modulo:
                if (IsNumericType(left) && IsNumericType(right))
                {
                    return GetNumericConversion(GetCommonNumericType(left, right));
                }
                break;
            case BinaryOperation.BitwiseAnd:
            case BinaryOperation.BitwiseOr:
            case BinaryOperation.BitwiseXor:
            case BinaryOperation.ShiftLeft:
            case BinaryOperation.ShiftRight:
            case BinaryOperation.UnsignedShiftRight:
                if (IsIntegralType(left) && IsIntegralType(right))
                {
                    return GetNumericConversion(GetCommonNumericType(left, right));
                }
                break;
            case BinaryOperation.LogicalAnd:
            case BinaryOperation.LogicalOr:
                if (left.SpecialType == SpecialType.Bool && right.SpecialType == SpecialType.Bool)
                {
                    return new BinaryOperandConversion(
                        Conversion.GetTrivial(ConversionKind.Identity),
                        Conversion.GetTrivial(ConversionKind.Identity),
                        binder.Compilation.GetSpecialType(SpecialType.Bool)
                    );
                }
                break;
            case BinaryOperation.Equal:
            case BinaryOperation.NotEqual:
                if (left.SpecialType == SpecialType.Bool && right.SpecialType == SpecialType.Bool)
                {
                    return new BinaryOperandConversion(
                        Conversion.GetTrivial(ConversionKind.Identity),
                        Conversion.GetTrivial(ConversionKind.Identity),
                        left
                    );
                }

                if (IsNumericType(left) && IsNumericType(right))
                {
                    return GetNumericConversion(GetCommonNumericType(left, right));
                }

                if (IsCharacterType(left) && IsCharacterType(right))
                {
                    var promoted = GetCommonCharacterType(left, right);
                    return new BinaryOperandConversion(
                        Conversion.GetTrivial(ConversionKind.Identity),
                        Conversion.GetTrivial(ConversionKind.Identity),
                        promoted
                    );
                }

                break;
            case BinaryOperation.LessThan:
            case BinaryOperation.LessThanOrEqual:
            case BinaryOperation.GreaterThan:
            case BinaryOperation.GreaterThanOrEqual:
            case BinaryOperation.ThreeWayComparison:
                if (IsNumericType(left) && IsNumericType(right))
                {
                    return GetNumericConversion(GetCommonNumericType(left, right));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }

        return null;

        BinaryOperandConversion? GetNumericConversion(TypeSymbol? promoted)
        {
            if (promoted is null)
                return null;

            return new BinaryOperandConversion(
                ClassifyNumericConversion(left.SpecialType, promoted.SpecialType),
                ClassifyNumericConversion(right.SpecialType, promoted.SpecialType),
                promoted
            );
        }
    }

    private Compilation Compilation => binder.Compilation;

    private static bool IsNumericType(TypeSymbol type)
    {
        return type.SpecialType.IsNumeric;
    }

    private static bool IsIntegralType(TypeSymbol type)
    {
        return type.SpecialType.IsInteger;
    }

    private Conversion ClassifyNumericConversion(SpecialType source, SpecialType destination)
    {
        if (source == destination)
        {
            return Conversion.GetTrivial(ConversionKind.Identity);
        }

        return Conversion.GetTrivial(
            IsImplicitNumericConversion(source, destination)
                ? ConversionKind.ImplicitNumeric
                : ConversionKind.ExplicitNumeric
        );
    }

    private bool IsImplicitNumericConversion(SpecialType source, SpecialType destination)
    {
        var sourceInfo = GetNumericInfo(source);
        var destinationInfo = GetNumericInfo(destination);

        if (sourceInfo.Family == NumericFamily.None || destinationInfo.Family == NumericFamily.None)
        {
            return false;
        }

        if (
            (
                sourceInfo.Family == NumericFamily.SignedInteger
                && destinationInfo.Family == NumericFamily.SignedInteger
            )
            || (
                sourceInfo.Family == NumericFamily.UnsignedInteger
                && destinationInfo.Family == NumericFamily.UnsignedInteger
            )
        )
        {
            return sourceInfo.Width <= destinationInfo.Width;
        }

        if (
            sourceInfo.Family == NumericFamily.UnsignedInteger
            && destinationInfo.Family == NumericFamily.SignedInteger
        )
        {
            return sourceInfo.Width < destinationInfo.Width;
        }

        if (
            IsIntegerFamily(sourceInfo.Family)
            && destinationInfo.Family == NumericFamily.FloatingPoint
        )
        {
            return sourceInfo.Precision <= destinationInfo.Precision;
        }

        if (
            sourceInfo.Family == NumericFamily.FloatingPoint
            && destinationInfo.Family == NumericFamily.FloatingPoint
        )
        {
            return sourceInfo.Width <= destinationInfo.Width;
        }

        return false;
    }

    private NumericInfo GetNumericInfo(SpecialType type)
    {
        return type switch
        {
            SpecialType.I8 => new NumericInfo(NumericFamily.SignedInteger, 8),
            SpecialType.I16 => new NumericInfo(NumericFamily.SignedInteger, 16),
            SpecialType.I32 => new NumericInfo(NumericFamily.SignedInteger, 32),
            SpecialType.I64 => new NumericInfo(NumericFamily.SignedInteger, 64),
            SpecialType.I128 => new NumericInfo(NumericFamily.SignedInteger, 128),
            SpecialType.ISize => new NumericInfo(
                NumericFamily.SignedInteger,
                Compilation.Settings.PointerWidth.BitWidth
            ),
            SpecialType.U8 => new NumericInfo(NumericFamily.UnsignedInteger, 8),
            SpecialType.U16 => new NumericInfo(NumericFamily.UnsignedInteger, 16),
            SpecialType.U32 => new NumericInfo(NumericFamily.UnsignedInteger, 32),
            SpecialType.U64 => new NumericInfo(NumericFamily.UnsignedInteger, 64),
            SpecialType.U128 => new NumericInfo(NumericFamily.UnsignedInteger, 128),
            SpecialType.USize => new NumericInfo(
                NumericFamily.UnsignedInteger,
                Compilation.Settings.PointerWidth.BitWidth
            ),
            SpecialType.F32 => new NumericInfo(NumericFamily.FloatingPoint, 32, 24),
            SpecialType.F64 => new NumericInfo(NumericFamily.FloatingPoint, 64, 53),
            _ => default,
        };
    }

    private static bool IsIntegerFamily(NumericFamily family)
    {
        return family is NumericFamily.SignedInteger or NumericFamily.UnsignedInteger;
    }

    private TypeSymbol PromoteNumericType(TypeSymbol source)
    {
        Debug.Assert(source.SpecialType.IsNumeric);
        return source.SpecialType switch
        {
            SpecialType.I8 or SpecialType.I16 or SpecialType.U8 or SpecialType.U16 =>
                Compilation.GetSpecialType(SpecialType.I32),
            _ => source,
        };
    }

    private TypeSymbol? PromoteNegationType(TypeSymbol source)
    {
        Debug.Assert(source.SpecialType.IsNumeric);
        return source.SpecialType switch
        {
            SpecialType.I8 or SpecialType.I16 or SpecialType.U8 or SpecialType.U16 =>
                Compilation.GetSpecialType(SpecialType.I32),
            SpecialType.U32 => Compilation.GetSpecialType(SpecialType.I64),
            SpecialType.U64 => Compilation.GetSpecialType(SpecialType.I128),
            SpecialType.USize => Compilation.Settings.PointerWidth switch
            {
                PointerWidth.X32 => Compilation.GetSpecialType(SpecialType.I64),
                PointerWidth.X64 => Compilation.GetSpecialType(SpecialType.I128),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            SpecialType.U128 => null,
            _ => source,
        };
    }

    private TypeSymbol? GetCommonNumericType(TypeSymbol left, TypeSymbol right)
    {
        Debug.Assert(left.SpecialType.IsNumeric && right.SpecialType.IsNumeric);

        if (left.SpecialType == right.SpecialType)
            return PromoteNumericType(left);

        var leftFamily = GetNumericInfo(left.SpecialType).Family;
        var rightFamily = GetNumericInfo(right.SpecialType).Family;

        if (leftFamily == rightFamily)
            return PromoteNumericType(WiderPrecision(left, right));

        if (IsIntegerFamily(leftFamily) && rightFamily == NumericFamily.FloatingPoint)
        {
            return CommonFloatType(right, left);
        }

        if (leftFamily == NumericFamily.FloatingPoint && IsIntegerFamily(rightFamily))
        {
            return CommonFloatType(left, right);
        }

        if (
            leftFamily == NumericFamily.SignedInteger
            && rightFamily == NumericFamily.UnsignedInteger
        )
        {
            var commonSigned = CommonSignedType(left, right);
            return commonSigned is not null ? PromoteNumericType(commonSigned) : null;
        }

        // ReSharper disable once InvertIf
        if (
            leftFamily == NumericFamily.UnsignedInteger
            && rightFamily == NumericFamily.SignedInteger
        )
        {
            var commonSigned = CommonSignedType(right, left);
            return commonSigned is not null ? PromoteNumericType(commonSigned) : null;
        }

        return null;
    }

    private TypeSymbol WiderPrecision(TypeSymbol left, TypeSymbol right)
    {
        var leftInfo = GetNumericInfo(left.SpecialType);
        var rightInfo = GetNumericInfo(right.SpecialType);
        Debug.Assert(leftInfo.Family == rightInfo.Family);

        return leftInfo.Precision < rightInfo.Precision ? right : left;
    }

    private TypeSymbol? CommonFloatType(TypeSymbol floatType, TypeSymbol integerType)
    {
        var floatInfo = GetNumericInfo(floatType.SpecialType);
        var integerInfo = GetNumericInfo(integerType.SpecialType);
        Debug.Assert(floatInfo.Family == NumericFamily.FloatingPoint);
        Debug.Assert(IsIntegerFamily(integerInfo.Family));

        if (floatInfo.Precision >= integerInfo.Precision)
            return floatType;

        if (
            floatType.SpecialType == SpecialType.F32
            && integerInfo.Precision <= GetNumericInfo(SpecialType.F64).Precision
        )
        {
            return Compilation.GetSpecialType(SpecialType.F64);
        }

        return null;
    }

    private TypeSymbol? CommonSignedType(TypeSymbol signedType, TypeSymbol unsignedType)
    {
        var signedInfo = GetNumericInfo(signedType.SpecialType);
        var unsignedInfo = GetNumericInfo(unsignedType.SpecialType);
        Debug.Assert(signedInfo.Family == NumericFamily.SignedInteger);
        Debug.Assert(unsignedInfo.Family == NumericFamily.UnsignedInteger);

        if (signedInfo.Precision > unsignedInfo.Precision)
            return signedType;

        return unsignedType.SpecialType switch
        {
            SpecialType.U8 => Compilation.GetSpecialType(SpecialType.I16),
            SpecialType.U16 => Compilation.GetSpecialType(SpecialType.I32),
            SpecialType.U32 => Compilation.GetSpecialType(SpecialType.I64),
            SpecialType.U64 => Compilation.GetSpecialType(SpecialType.I128),
            SpecialType.U128 => (TypeSymbol?)null,
            SpecialType.USize => Compilation.Settings.PointerWidth switch
            {
                PointerWidth.X32 => Compilation.GetSpecialType(SpecialType.I64),
                PointerWidth.X64 => Compilation.GetSpecialType(SpecialType.I128),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            _ => throw new InvalidOperationException("Invalid unsigned type"),
        };
    }

    private static bool IsCharacterType(TypeSymbol type)
    {
        return type.SpecialType.IsCharacter;
    }

    private static int CharacterWidth(SpecialType type)
    {
        Debug.Assert(type.IsCharacter);
        return type switch
        {
            SpecialType.Char => 8,
            SpecialType.Char16 => 16,
            SpecialType.Rune => 32,
            _ => throw new InvalidOperationException("Invalid character type"),
        };
    }

    private static Conversion ClassifyCharacterConversion(
        SpecialType source,
        SpecialType destination
    )
    {
        if (source == destination)
            return Conversion.GetTrivial(ConversionKind.Identity);

        var sourceWidth = CharacterWidth(source);
        var destinationWidth = CharacterWidth(destination);
        Debug.Assert(sourceWidth != destinationWidth);

        return sourceWidth > destinationWidth
            ? Conversion.GetTrivial(ConversionKind.ExplicitCharacter)
            : Conversion.GetTrivial(ConversionKind.ImplicitCharacter);
    }

    private static TypeSymbol GetCommonCharacterType(TypeSymbol left, TypeSymbol right)
    {
        if (left == right)
            return left;

        var sourceWidth = CharacterWidth(left.SpecialType);
        var destinationWidth = CharacterWidth(right.SpecialType);
        Debug.Assert(sourceWidth != destinationWidth);

        return sourceWidth > destinationWidth ? left : right;
    }

    private enum NumericFamily : byte
    {
        None,
        SignedInteger,
        UnsignedInteger,
        FloatingPoint,
    };

    private readonly record struct NumericInfo(NumericFamily Family, int Width, int Precision)
    {
        public NumericInfo(NumericFamily family, int width)
            : this(family, width, width) { }
    }
}
