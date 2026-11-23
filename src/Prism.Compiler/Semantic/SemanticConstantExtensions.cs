using Prism.Compiler.Mappers;
using Prism.Configuration;
using Prism.Constants;

namespace Prism.Compiler.Semantic;

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
}
