using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal abstract record BoundExpression : BoundNode
{
    private Lazy<ConstantValue?>? _constantValue;

    protected BoundExpression(SyntaxNode syntax, TypeSymbol type)
        : base(syntax)
    {
        Type = type;
    }

    public TypeSymbol Type { get; }

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

internal sealed record BoundBadExpression : BoundExpression
{
    public BoundBadExpression(SyntaxNode syntax, TypeSymbol type)
        : base(syntax, type) { }
}

internal sealed record BoundLiteral : BoundExpression
{
    public BoundLiteral(SyntaxNode syntax, TypeSymbol type, ConstantValue value)
        : base(syntax, type)
    {
        Value = value;
    }

    public ConstantValue Value { get; }

    protected override ConstantValue? ComputeConstantValue() => Value;
}

internal sealed record BoundVariableAccess : BoundExpression
{
    public BoundVariableAccess(SyntaxNode syntax, VariableSymbol symbol)
        : base(syntax, symbol.Type)
    {
        Symbol = symbol;
    }

    public VariableSymbol Symbol { get; }

    public override bool IsAddressable => true;

    public override bool IsAssignable => Symbol.IsMutable;
}

internal sealed record BoundParameterAccess : BoundExpression
{
    public BoundParameterAccess(SyntaxNode syntax, ParameterSymbol symbol)
        : base(syntax, symbol.Type)
    {
        Symbol = symbol;
    }

    public ParameterSymbol Symbol { get; }

    public override bool IsAddressable => true;

    public override bool IsAssignable => Symbol.IsMutable;
}

internal sealed record BoundUnaryOperation : BoundExpression
{
    public BoundUnaryOperation(
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression operand,
        UnaryOperation operation
    )
        : base(syntax, type)
    {
        Operand = operand;
        Operation = operation;
    }

    public BoundExpression Operand { get; }

    public UnaryOperation Operation { get; }
}

internal sealed record BoundBinaryOperation : BoundExpression
{
    public BoundBinaryOperation(
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression left,
        BoundExpression right,
        BinaryOperation operation
    )
        : base(syntax, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public BoundExpression Left { get; }
    public BoundExpression Right { get; }
    public BinaryOperation Operation { get; }
}

internal sealed record BoundAssignmentOperation : BoundExpression
{
    public BoundAssignmentOperation(
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression left,
        BoundExpression right,
        AssignmentOperation operation
    )
        : base(syntax, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public BoundExpression Left { get; }
    public BoundExpression Right { get; }
    public AssignmentOperation Operation { get; }
}

internal sealed record BoundConditional : BoundExpression
{
    public BoundConditional(
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression condition,
        BoundExpression whenTrue,
        BoundExpression whenFalse
    )
        : base(syntax, type)
    {
        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
    }

    public BoundExpression Condition { get; }
    public BoundExpression WhenTrue { get; }
    public BoundExpression WhenFalse { get; }
}

internal sealed record BoundInvocation : BoundExpression
{
    public BoundInvocation(
        SyntaxNode syntax,
        FunctionSymbol function,
        ImmutableArray<BoundExpression> arguments
    )
        : base(syntax, function.ReturnType)
    {
        Function = function;
        Arguments = arguments;
    }

    public FunctionSymbol Function { get; }

    public ImmutableArray<BoundExpression> Arguments { get; }
}

internal sealed record BoundConversion : BoundExpression
{
    public BoundConversion(
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression operand,
        Conversion conversion
    )
        : base(syntax, type)
    {
        Operand = operand;
        Conversion = conversion;
    }

    public BoundExpression Operand { get; }

    public Conversion Conversion { get; }
}

internal sealed record BoundAddressOf : BoundExpression
{
    public BoundAddressOf(SyntaxNode syntax, BoundExpression operand, TypeSymbol type)
        : base(syntax, type)
    {
        Operand = operand;
    }

    public BoundExpression Operand { get; }
}

internal sealed record BoundDereference : BoundExpression
{
    public BoundDereference(
        SyntaxNode syntax,
        BoundExpression operand,
        TypeSymbol type,
        bool isMutable
    )
        : base(syntax, type)
    {
        Operand = operand;
        IsAssignable = isMutable;
    }

    public BoundExpression Operand { get; }

    public override bool IsAddressable => true;

    public override bool IsAssignable { get; }
}

internal sealed record BoundIndex : BoundExpression
{
    public BoundIndex(
        SyntaxNode syntax,
        BoundExpression operand,
        BoundExpression index,
        TypeSymbol type
    )
        : base(syntax, type)
    {
        Operand = operand;
        Index = index;
    }

    public BoundExpression Operand { get; }
    public BoundExpression Index { get; }
}
