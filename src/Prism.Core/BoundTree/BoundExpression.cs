using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract class BoundExpression(ExpressionSyntax syntax, TypeSymbol type)
    : BoundNode(syntax)
{
    private Lazy<ConstantValue?>? _constantValue;

    public TypeSymbol Type { get; } = type;

    public virtual bool IsAddressable => false;

    public virtual bool IsAssignable => false;

    public ConstantValue? ConstantValue
    {
        get
        {
            if (_constantValue is not null)
                return _constantValue.Value;

            Interlocked.CompareExchange(
                ref _constantValue,
                new Lazy<ConstantValue?>(
                    ComputeConstantValue,
                    LazyThreadSafetyMode.PublicationOnly
                ),
                null
            );
            return _constantValue.Value;
        }
    }

    protected virtual ConstantValue? ComputeConstantValue() => null;
}

internal sealed class BoundBadExpression(ExpressionSyntax syntax, TypeSymbol type)
    : BoundExpression(syntax, type);

internal sealed class BoundLiteral(ExpressionSyntax syntax, TypeSymbol type, ConstantValue value)
    : BoundExpression(syntax, type)
{
    public ConstantValue Value { get; } = value;

    protected override ConstantValue? ComputeConstantValue() => Value;
}

internal sealed class BoundVariableAccess(ExpressionSyntax syntax, VariableSymbol symbol)
    : BoundExpression(syntax, symbol.Type)
{
    public VariableSymbol Symbol { get; } = symbol;

    public override bool IsAddressable => true;

    public override bool IsAssignable => Symbol.IsMutable;
}

internal sealed class BoundParameterAccess(ExpressionSyntax syntax, ParameterSymbol symbol)
    : BoundExpression(syntax, symbol.Type)
{
    public ParameterSymbol Symbol { get; } = symbol;

    public override bool IsAddressable => true;

    public override bool IsAssignable => Symbol.IsMutable;
}

internal sealed class BoundUnaryOperation(
    ExpressionSyntax syntax,
    TypeSymbol type,
    BoundExpression operand,
    UnaryOperation operation
) : BoundExpression(syntax, type)
{
    public BoundExpression Operand { get; } = operand;

    public UnaryOperation Operation { get; } = operation;
}

internal sealed class BoundBinaryOperation(
    ExpressionSyntax syntax,
    TypeSymbol type,
    BoundExpression left,
    BoundExpression right,
    BinaryOperation operation
) : BoundExpression(syntax, type)
{
    public BoundExpression Left { get; } = left;
    public BoundExpression Right { get; } = right;
    public BinaryOperation Operation { get; } = operation;
}

internal sealed class BoundAssignmentOperation(
    ExpressionSyntax syntax,
    TypeSymbol type,
    BoundExpression left,
    BoundExpression right,
    AssignmentOperation operation
) : BoundExpression(syntax, type)
{
    public BoundExpression Left { get; } = left;
    public BoundExpression Right { get; } = right;
    public AssignmentOperation Operation { get; } = operation;
}

internal sealed class BoundConditional(
    ExpressionSyntax syntax,
    TypeSymbol type,
    BoundExpression condition,
    BoundExpression whenTrue,
    BoundExpression whenFalse
) : BoundExpression(syntax, type)
{
    public BoundExpression Condition { get; } = condition;
    public BoundExpression WhenTrue { get; } = whenTrue;
    public BoundExpression WhenFalse { get; } = whenFalse;
}

internal sealed class BoundInvocation(
    ExpressionSyntax syntax,
    FunctionSymbol function,
    ImmutableArray<BoundExpression> arguments
) : BoundExpression(syntax, function.ReturnType)
{
    public FunctionSymbol Function { get; } = function;

    public ImmutableArray<BoundExpression> Arguments { get; } = arguments;
}

internal sealed class BoundConversion(
    ExpressionSyntax syntax,
    TypeSymbol type,
    BoundExpression operand,
    Conversion conversion
) : BoundExpression(syntax, type)
{
    public BoundExpression Operand { get; } = operand;

    public Conversion Conversion { get; } = conversion;
}

internal sealed class BoundAddressOf(
    PrefixExpressionSyntax syntax,
    BoundExpression operand,
    TypeSymbol type
) : BoundExpression(syntax, type)
{
    public BoundExpression Operand { get; } = operand;
}

internal sealed class BoundDereference(
    PrefixExpressionSyntax syntax,
    BoundExpression operand,
    TypeSymbol type,
    bool isMutable
) : BoundExpression(syntax, type)
{
    public BoundExpression Operand { get; } = operand;

    public override bool IsAddressable => true;

    public override bool IsAssignable { get; } = isMutable;
}

internal sealed class BoundIndex(
    IndexExpressionSyntax syntax,
    BoundExpression operand,
    BoundExpression index,
    TypeSymbol type
) : BoundExpression(syntax, type)
{
    public BoundExpression Operand { get; } = operand;
    public BoundExpression Index { get; } = index;
}
