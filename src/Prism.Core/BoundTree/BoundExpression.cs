using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Symbols.Intermediate;
using Prism.Core.Syntax;

namespace Prism.Core.BoundTree;

internal closed record BoundExpression : BoundNode
{
    private Lazy<ConstantValue?>? _constantValue;

    protected BoundExpression(Compilation compilation, SyntaxNode syntax, TypeSymbol type)
        : base(compilation, syntax)
    {
        Type = type;
    }

    public TypeSymbol Type { get; }

    public virtual bool IsAddressable => false;

    /// <summary>
    /// Indicates if the expression represents a value that can be assigned to.
    /// </summary>
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
    public BoundBadExpression(Compilation compilation, SyntaxNode syntax, TypeSymbol type)
        : base(compilation, syntax, type)
    {
        HasErrors = true;
    }
}

internal closed record BoundSpeculativeExpression : BoundExpression
{
    protected BoundSpeculativeExpression(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        TypeSymbol defaultType
    )
        : base(compilation, syntax, type)
    {
        DefaultType = defaultType;
    }

    public TypeSymbol DefaultType { get; }
}

internal sealed record BoundUnfixedIntegerLiteral : BoundSpeculativeExpression
{
    public BoundUnfixedIntegerLiteral(
        Compilation compilation,
        SyntaxNode syntax,
        IntegerLiteralData data,
        TypeSymbol defaultType
    )
        : base(compilation, syntax, UnfixedIntegerTypeSymbol.Instance, defaultType)
    {
        Data = data;
    }

    public IntegerLiteralData Data { get; }

    public bool Negated { get; init; }
}

internal sealed record BoundUnfixedFloatLiteral : BoundSpeculativeExpression
{
    public BoundUnfixedFloatLiteral(
        Compilation compilation,
        SyntaxNode syntax,
        FloatLiteralData data,
        TypeSymbol defaultType
    )
        : base(compilation, syntax, UnfixedFloatTypeSymbol.Instance, defaultType)
    {
        Data = data;
    }

    public FloatLiteralData Data { get; }

    public bool Negated { get; init; }
}

internal sealed record BoundLiteral : BoundExpression
{
    public BoundLiteral(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        ConstantValue value
    )
        : base(compilation, syntax, type)
    {
        Value = value;
    }

    public ConstantValue Value { get; }

    protected override ConstantValue? ComputeConstantValue() => Value;
}

internal sealed record BoundThisExpression : BoundExpression
{
    public BoundThisExpression(Compilation compilation, SyntaxNode syntax, TypeSymbol type)
        : base(compilation, syntax, type) { }
}

internal sealed record BoundTypeSize : BoundExpression
{
    public BoundTypeSize(Compilation compilation, SyntaxNode syntax, TypeSymbol targetType)
        : base(compilation, syntax, compilation.GetSpecialType(SpecialType.USize))
    {
        TargetType = targetType;
    }

    public TypeSymbol TargetType { get; }

    protected override ConstantValue? ComputeConstantValue()
    {
        if (TargetType.IsDynamicallySized)
            return null;

        var layout = Compilation.GetTypeLayout(TargetType);
        return new USizeConstant(layout.Size);
    }
}

internal sealed record BoundVariableAccess : BoundExpression
{
    public BoundVariableAccess(Compilation compilation, SyntaxNode syntax, VariableSymbol symbol)
        : base(compilation, syntax, symbol.Type)
    {
        Symbol = symbol;
    }

    public BoundVariableAccess(
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression? owner,
        VariableSymbol symbol
    )
        : base(compilation, syntax, symbol.Type)
    {
        Owner = owner;
        Symbol = symbol;
    }

    public BoundExpression? Owner { get; }

    public VariableSymbol Symbol { get; }

    public override bool IsAddressable => true;

    /// <inheritdoc/>
    public override bool IsAssignable => true;

    protected override ConstantValue? ComputeConstantValue() => Symbol.ConstantValue;
}

internal sealed record BoundParameterAccess : BoundExpression
{
    public BoundParameterAccess(Compilation compilation, SyntaxNode syntax, ParameterSymbol symbol)
        : base(compilation, syntax, symbol.Type)
    {
        Symbol = symbol;
    }

    public ParameterSymbol Symbol { get; }

    public override bool IsAddressable => true;

    public override bool IsAssignable => true;
}

internal sealed record BoundUnaryOperation : BoundExpression
{
    public BoundUnaryOperation(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression operand,
        UnaryOperation operation
    )
        : base(compilation, syntax, type)
    {
        Operand = operand;
        Operation = operation;
    }

    public BoundExpression Operand { get; }

    public UnaryOperation Operation { get; }

    protected override ConstantValue? ComputeConstantValue()
    {
        return Operation switch
        {
            UnaryOperation.Identity => Operand.ConstantValue,
            UnaryOperation.Negation => Operand.ConstantValue?.TryNegate(Compilation.Settings),
            UnaryOperation.LogicalNot => Operand.ConstantValue?.TryLogicalNot(),
            UnaryOperation.BitwiseNot => Operand.ConstantValue?.TryBitwiseNot(Compilation.Settings),
            _ => null,
        };
    }
}

internal sealed record BoundBinaryOperation : BoundExpression
{
    public BoundBinaryOperation(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression left,
        BoundExpression right,
        BinaryOperation operation
    )
        : base(compilation, syntax, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public BoundExpression Left { get; }
    public BoundExpression Right { get; }
    public BinaryOperation Operation { get; }

    protected override ConstantValue? ComputeConstantValue()
    {
        if (Left.ConstantValue is not { } leftConst || Right.ConstantValue is not { } rightConst)
            return null;

        return leftConst.TryBinary(Operation, in rightConst, Compilation.Settings);
    }
}

internal sealed record BoundAssignmentOperation : BoundExpression
{
    public BoundAssignmentOperation(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression left,
        BoundExpression right,
        AssignmentOperation operation
    )
        : base(compilation, syntax, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public BoundExpression Left { get; }
    public BoundExpression Right { get; }
    public AssignmentOperation Operation { get; }
}

internal sealed record BoundSpeculativeConditional : BoundSpeculativeExpression
{
    public BoundSpeculativeConditional(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression condition,
        BoundExpression whenTrue,
        BoundExpression whenFalse
    )
        : base(
            compilation,
            syntax,
            type,
            whenTrue is BoundSpeculativeExpression speculative
                ? speculative.DefaultType
                : whenTrue.Type
        )
    {
        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
    }

    public BoundExpression Condition { get; }
    public BoundExpression WhenTrue { get; }
    public BoundExpression WhenFalse { get; }
}

internal sealed record BoundConditional : BoundExpression
{
    public BoundConditional(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression condition,
        BoundExpression whenTrue,
        BoundExpression whenFalse
    )
        : base(compilation, syntax, type)
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
        Compilation compilation,
        SyntaxNode syntax,
        FunctionSymbol function,
        BoundExpression? receiver,
        ImmutableArray<BoundExpression> arguments
    )
        : base(compilation, syntax, function.ReturnType)
    {
        Function = function;
        Receiver = receiver;
        Arguments = arguments;
    }

    public FunctionSymbol Function { get; }

    public BoundExpression? Receiver { get; }

    public ImmutableArray<BoundExpression> Arguments { get; }
}

internal sealed record BoundConversion : BoundExpression
{
    public BoundConversion(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        BoundExpression operand,
        Conversion conversion
    )
        : base(compilation, syntax, type)
    {
        Operand = operand;
        Conversion = conversion;
    }

    public BoundExpression Operand { get; }

    public Conversion Conversion { get; }

    protected override ConstantValue? ComputeConstantValue()
    {
        return Operand.ConstantValue?.TryConvert(Type, Compilation.Settings);
    }
}

internal sealed record BoundAddressOf : BoundExpression
{
    public BoundAddressOf(
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression operand,
        TypeSymbol type,
        bool isMutable
    )
        : base(compilation, syntax, type)
    {
        Operand = operand;
        IsMutable = isMutable;
    }

    public BoundExpression Operand { get; }

    public bool IsMutable { get; }
}

internal sealed record BoundDereference : BoundExpression
{
    public BoundDereference(
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression operand,
        TypeSymbol type,
        bool isMutable
    )
        : base(compilation, syntax, type)
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
        Compilation compilation,
        SyntaxNode syntax,
        BoundExpression operand,
        BoundExpression index,
        TypeSymbol type
    )
        : base(compilation, syntax, type)
    {
        Operand = operand;
        Index = index;
    }

    public BoundExpression Operand { get; }
    public BoundExpression Index { get; }
}

internal sealed record BoundCollectionExpression : BoundExpression
{
    public BoundCollectionExpression(
        Compilation compilation,
        SyntaxNode syntax,
        TypeSymbol type,
        TypeSymbol elementType,
        ImmutableArray<BoundExpression> expressions
    )
        : base(compilation, syntax, type)
    {
        ElementType = elementType;
        Expressions = expressions;
    }

    public TypeSymbol ElementType { get; }
    public ImmutableArray<BoundExpression> Expressions { get; }
}

internal sealed record BoundSpeculativeCollectionExpression : BoundSpeculativeExpression
{
    public BoundSpeculativeCollectionExpression(
        Compilation compilation,
        SyntaxNode syntax,
        ImmutableArray<BoundExpression> expressions,
        TypeSymbol defaultType
    )
        : base(compilation, syntax, UndeterminedCollectionTypeSymbol.Instance, defaultType)
    {
        Expressions = expressions;
    }

    public ImmutableArray<BoundExpression> Expressions { get; }
}
