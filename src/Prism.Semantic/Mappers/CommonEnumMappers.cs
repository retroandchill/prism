using Prism.Configuration;
using Prism.Constants;
using Riok.Mapperly.Abstractions;

namespace Prism.Semantic.Mappers;

[Mapper(
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    RequiredMappingStrategy = RequiredMappingStrategy.Source
)]
internal static partial class CommonEnumMappers
{
    public static partial ConstantBinaryOperation ToConstantOperation(this BinaryOperation op);

    [MapperIgnoreSourceValue(BinaryOperation.Equality)]
    [MapperIgnoreSourceValue(BinaryOperation.NotEquals)]
    [MapperIgnoreSourceValue(BinaryOperation.LessThan)]
    [MapperIgnoreSourceValue(BinaryOperation.LessThanOrEquals)]
    [MapperIgnoreSourceValue(BinaryOperation.GreaterThan)]
    [MapperIgnoreSourceValue(BinaryOperation.GreaterThanOrEquals)]
    [MapperIgnoreSourceValue(BinaryOperation.ThreeWayComparison)]
    public static partial AssignmentOperation ToAssignmentOperation(this BinaryOperation kind);

    [MapperIgnoreSourceValue(AssignmentOperation.Simple)]
    public static partial BinaryOperation ToBinaryOperation(this AssignmentOperation operation);

    extension(SpecialType type)
    {
        public IntegerKind ToIntegerKind(PointerWidth pointerWidth)
        {
            return type switch
            {
                SpecialType.I8 => IntegerKind.I8,
                SpecialType.I16 => IntegerKind.I16,
                SpecialType.I32 => IntegerKind.I32,
                SpecialType.I64 => IntegerKind.I64,
                SpecialType.I128 => IntegerKind.I128,
                SpecialType.ISize => pointerWidth switch
                {
                    PointerWidth.X32 => IntegerKind.I32,
                    PointerWidth.X64 => IntegerKind.I64,
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                },
                SpecialType.U8 => IntegerKind.U8,
                SpecialType.U16 => IntegerKind.U16,
                SpecialType.U32 => IntegerKind.U32,
                SpecialType.U64 => IntegerKind.U64,
                SpecialType.U128 => IntegerKind.U128,
                SpecialType.USize => pointerWidth switch
                {
                    PointerWidth.X32 => IntegerKind.U32,
                    PointerWidth.X64 => IntegerKind.U64,
                    _ => throw new InvalidOperationException("Invalid pointer width"),
                },
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Special type must be an integer type"
                ),
            };
        }
    }
}
