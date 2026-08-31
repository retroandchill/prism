using Prism.Core.Binding;
using Prism.Core.Symbols;

namespace Prism.Core.Semantic;

internal readonly record struct OperandConversion(Conversion Conversion, TypeSymbol Type);

internal readonly record struct BinaryOperandConversion(Conversion LeftConversion, TypeSymbol Type);

internal sealed class ConversionClassifier(Binder binder)
{
    public Conversion ClassifyConversion(TypeSymbol source, TypeSymbol target)
    {
        throw new NotImplementedException();
    }

    public OperandConversion? ClassifyUnaryOperand(UnaryOperation operation, TypeSymbol operand)
    {
        throw new NotImplementedException();
    }

    public BinaryOperandConversion? ClassifyBinaryOperand(
        BinaryOperation operation,
        TypeSymbol left,
        TypeSymbol right
    )
    {
        throw new NotImplementedException();
    }
}
