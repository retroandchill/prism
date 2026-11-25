using Prism.Configuration;
using Prism.Constants;
using Prism.Semantic.Mappers;
using Prism.Semantic.Symbols;

namespace Prism.Semantic;

internal static class SemanticConstantExtensions
{
    extension(ConstantValue)
    {
        public static ConstantValue Zero(SpecialType specialType, PointerWidth pointerWidth)
        {
            return specialType switch
            {
                { IsInteger: true } => IntegerConstant.Zero(
                    specialType.ToIntegerKind(pointerWidth)
                ),
                SpecialType.F32 => FloatConstant.Zero(FloatKind.F32),
                SpecialType.F64 => FloatConstant.Zero(FloatKind.F64),
                SpecialType.Char => CharacterConstant.Zero(CharacterKind.C8),
                SpecialType.Char16 => CharacterConstant.Zero(CharacterKind.C16),
                SpecialType.Rune => CharacterConstant.Zero(CharacterKind.C32),
                _ => throw new InvalidOperationException("Cannot create a zero"),
            };
        }
    }

    extension(in ConstantValue value)
    {
        public ConstantValue? TryConvert(TypeSymbol typeSymbol, PointerWidth pointerWidth)
        {
            while (true)
            {
                if (typeSymbol is NullableTypeSymbol nullableType)
                {
                    if (value.IsNull)
                        return value;
                    typeSymbol = nullableType.ElementType;
                    continue;
                }

                if (value.IsNull)
                {
                    return null;
                }

                if (typeSymbol.SpecialType.IsCharacter)
                {
                    return value.TryConvertToCharacter(typeSymbol.SpecialType);
                }

                return typeSymbol.SpecialType switch
                {
                    SpecialType.Bool => value is bool ? value : (ConstantValue?)null,
                    { IsCharacter: true } => value.TryConvertToCharacter(typeSymbol.SpecialType),

                    { IsInteger: true } => value.TryConvertToInteger(
                        typeSymbol.SpecialType,
                        pointerWidth
                    ),

                    { IsFloatingPoint: true } => value.TryConvertToFloat(typeSymbol.SpecialType),

                    SpecialType.Str => value is string ? value : (ConstantValue?)null,

                    _ => null,
                };
            }
        }

        private ConstantValue? TryConvertToCharacter(SpecialType type)
        {
            return value switch
            {
                CharacterConstant ch => ch.ConvertTo(ToCharacterKind(type)),
                _ => null,
            };
        }

        private IntegerKind ToIntegerKind(SpecialType type, PointerWidth width)
        {
            return type switch
            {
                SpecialType.I8 => IntegerKind.I8,
                SpecialType.I16 => IntegerKind.I16,
                SpecialType.I32 => IntegerKind.I32,
                SpecialType.I64 => IntegerKind.I64,
                SpecialType.I128 => IntegerKind.I128,
                SpecialType.ISize => width switch
                {
                    PointerWidth.X32 => IntegerKind.I32,
                    PointerWidth.X64 => IntegerKind.I64,
                    _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
                },
                SpecialType.U8 => IntegerKind.U8,
                SpecialType.U16 => IntegerKind.U16,
                SpecialType.U32 => IntegerKind.U32,
                SpecialType.U64 => IntegerKind.U64,
                SpecialType.U128 => IntegerKind.U128,
                SpecialType.USize => width switch
                {
                    PointerWidth.X32 => IntegerKind.U32,
                    PointerWidth.X64 => IntegerKind.U64,
                    _ => throw new ArgumentOutOfRangeException(nameof(width), width, null),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        private ConstantValue? TryConvertToInteger(SpecialType type, PointerWidth pointerWidth)
        {
            var integerKind = value.ToIntegerKind(type, pointerWidth);
            return value switch
            {
                IntegerConstant i => i.ConvertTo(integerKind),
                FloatConstant f => f.ConvertTo(integerKind),
                _ => null,
            };
        }

        private ConstantValue? TryConvertToFloat(SpecialType type)
        {
            var floatKind = ToFloatKind(type);
            return value switch
            {
                IntegerConstant i => i.ConvertTo((FloatKind)floatKind),
                FloatConstant f => f.ConvertTo((FloatKind)floatKind),
                _ => null,
            };
        }
    }

    private static CharacterKind ToCharacterKind(SpecialType type)
    {
        return type switch
        {
            SpecialType.Char => CharacterKind.C8,
            SpecialType.Char16 => CharacterKind.C16,
            SpecialType.Rune => CharacterKind.C32,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    private static FloatKind ToFloatKind(SpecialType type)
    {
        return type switch
        {
            SpecialType.F32 => FloatKind.F32,
            SpecialType.F64 => FloatKind.F64,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
