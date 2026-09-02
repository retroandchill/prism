using Prism.Core.Binding;
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

    extension(SyntaxKind kind)
    {
        public SpecialType ToSpecialType()
        {
            return kind switch
            {
                SyntaxKind.VoidKeyword => SpecialType.Void,
                SyntaxKind.BoolKeyword => SpecialType.Bool,
                SyntaxKind.I8Keyword => SpecialType.I8,
                SyntaxKind.I16Keyword => SpecialType.I16,
                SyntaxKind.I32Keyword => SpecialType.I32,
                SyntaxKind.I64Keyword => SpecialType.I64,
                SyntaxKind.I128Keyword => SpecialType.I128,
                SyntaxKind.U8Keyword => SpecialType.U8,
                SyntaxKind.U16Keyword => SpecialType.U16,
                SyntaxKind.U32Keyword => SpecialType.U32,
                SyntaxKind.U64Keyword => SpecialType.U64,
                SyntaxKind.U128Keyword => SpecialType.U128,
                SyntaxKind.ISizeKeyword => SpecialType.ISize,
                SyntaxKind.USizeKeyword => SpecialType.USize,
                SyntaxKind.F32Keyword => SpecialType.F32,
                SyntaxKind.F64Keyword => SpecialType.F64,
                SyntaxKind.CharKeyword => SpecialType.Char,
                SyntaxKind.Char16Keyword => SpecialType.Char16,
                SyntaxKind.RuneKeyword => SpecialType.Rune,
                SyntaxKind.StrKeyword => SpecialType.Str,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };
        }

        public UnaryOperation ToPrefixOperation()
        {
            return kind switch
            {
                SyntaxKind.PlusToken => UnaryOperation.Identity,
                SyntaxKind.MinusToken => UnaryOperation.Negation,
                SyntaxKind.BangToken => UnaryOperation.LogicalNot,
                SyntaxKind.TildeToken => UnaryOperation.BitwiseNot,
                SyntaxKind.PlusPlusToken => UnaryOperation.PreIncrement,
                SyntaxKind.MinusMinusToken => UnaryOperation.PreDecrement,
                _ => throw new ArgumentException("Not a prefix operator", nameof(kind)),
            };
        }

        public UnaryOperation ToPostfixOperation()
        {
            return kind switch
            {
                SyntaxKind.PlusPlusToken => UnaryOperation.PostIncrement,
                SyntaxKind.MinusMinusToken => UnaryOperation.PostDecrement,
                _ => throw new ArgumentException("Not a prefix operator", nameof(kind)),
            };
        }

        public BinaryOperation ToBinaryOperation()
        {
            return kind switch
            {
                SyntaxKind.PlusToken => BinaryOperation.Addition,
                SyntaxKind.MinusToken => BinaryOperation.Subtraction,
                SyntaxKind.StarToken => BinaryOperation.Multiplication,
                SyntaxKind.SlashToken => BinaryOperation.Division,
                SyntaxKind.PercentToken => BinaryOperation.Modulo,
                SyntaxKind.AmpToken => BinaryOperation.BitwiseAnd,
                SyntaxKind.PipeToken => BinaryOperation.BitwiseOr,
                SyntaxKind.CaretToken => BinaryOperation.BitwiseXor,
                SyntaxKind.AmpAmpToken => BinaryOperation.LogicalAnd,
                SyntaxKind.PipePipeToken => BinaryOperation.LogicalOr,
                SyntaxKind.EqualEqualToken => BinaryOperation.Equality,
                SyntaxKind.BangEqualToken => BinaryOperation.NotEquals,
                SyntaxKind.LessToken => BinaryOperation.LessThan,
                SyntaxKind.LessEqualToken => BinaryOperation.LessThanOrEquals,
                SyntaxKind.GreaterToken => BinaryOperation.GreaterThan,
                SyntaxKind.GreaterEqualToken => BinaryOperation.GreaterThanOrEquals,
                SyntaxKind.SpaceshipToken => BinaryOperation.ThreeWayComparison,
                SyntaxKind.LessLessToken => BinaryOperation.ShiftLeft,
                SyntaxKind.GreaterGreaterToken => BinaryOperation.ShiftRight,
                SyntaxKind.GreaterGreaterGreaterToken => BinaryOperation.UnsignedShiftRight,
                _ => throw new ArgumentException("invalid binary operation", nameof(kind)),
            };
        }

        public AssignmentOperation ToAssignmentOperation()
        {
            return kind switch
            {
                SyntaxKind.EqualToken => AssignmentOperation.Simple,
                SyntaxKind.PlusEqualToken => AssignmentOperation.Addition,
                SyntaxKind.MinusEqualToken => AssignmentOperation.Subtraction,
                SyntaxKind.StarEqualToken => AssignmentOperation.Multiplication,
                SyntaxKind.SlashEqualToken => AssignmentOperation.Division,
                SyntaxKind.PercentEqualToken => AssignmentOperation.Modulo,
                SyntaxKind.AmpEqualToken => AssignmentOperation.BitwiseAnd,
                SyntaxKind.PipeEqualToken => AssignmentOperation.BitwiseOr,
                SyntaxKind.CaretEqualToken => AssignmentOperation.BitwiseXor,
                SyntaxKind.AmpAmpEqualToken => AssignmentOperation.LogicalAnd,
                SyntaxKind.PipePipeEqualToken => AssignmentOperation.LogicalOr,
                SyntaxKind.LessLessEqualToken => AssignmentOperation.ShiftLeft,
                SyntaxKind.GreaterGreaterEqualToken => AssignmentOperation.ShiftRight,
                SyntaxKind.GreaterGreaterGreaterEqualToken =>
                    AssignmentOperation.UnsignedShiftRight,
                _ => throw new ArgumentException("invalid assignment operation", nameof(kind)),
            };
        }
    }

    [MapperIgnoreSourceValue(AssignmentOperation.Simple)]
    public static partial BinaryOperation ToBinaryOperation(this AssignmentOperation operation);
}
