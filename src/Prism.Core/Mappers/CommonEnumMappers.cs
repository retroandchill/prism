using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using Riok.Mapperly.Abstractions;

namespace Prism.Core.Mappers;

[Mapper(
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    RequiredMappingStrategy = RequiredMappingStrategy.Source
)]
internal static partial class CommonEnumMappers
{
    public static partial SpecialType ToSpecialType(this ConstantKind kind);

    [MapperIgnoreSourceValue(BinaryOperation.Equality)]
    [MapperIgnoreSourceValue(BinaryOperation.NotEquals)]
    [MapperIgnoreSourceValue(BinaryOperation.LessThan)]
    [MapperIgnoreSourceValue(BinaryOperation.LessThanOrEquals)]
    [MapperIgnoreSourceValue(BinaryOperation.GreaterThan)]
    [MapperIgnoreSourceValue(BinaryOperation.GreaterThanOrEquals)]
    [MapperIgnoreSourceValue(BinaryOperation.ThreeWayComparison)]
    public static partial AssignmentOperation ToAssignmentOperation(this BinaryOperation kind);
}
