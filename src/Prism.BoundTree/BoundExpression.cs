using System.Collections.Immutable;
using Prism.BoundTree.Symbols.Intermediate;
using Prism.Configuration;
using Prism.Constants;
using Prism.Semantic;
using Prism.Semantic.Conversions;
using Prism.Semantic.Mappers;
using Prism.Semantic.Symbols;
using Prism.Utils;

namespace Prism.BoundTree;

internal closed record BoundExpression : BoundNode
{
    private Lazy<ConstantValue?>? _constantValue;

    protected BoundExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type
    )
        : base(semanticContext, syntax)
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
    public BoundBadExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type
    )
        : base(semanticContext, syntax, type)
    {
        HasErrors = true;
    }
}

internal closed record BoundSpeculativeExpression : BoundExpression
{
    protected BoundSpeculativeExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        TypeSymbol defaultType
    )
        : base(semanticContext, syntax, type)
    {
        DefaultType = defaultType;
    }

    public TypeSymbol DefaultType { get; }
}

internal sealed record BoundUnfixedIntegerLiteral : BoundSpeculativeExpression
{
    public BoundUnfixedIntegerLiteral(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        RawIntegerLiteralData data,
        TypeSymbol defaultType
    )
        : base(semanticContext, syntax, UnfixedIntegerTypeSymbol.Instance.PublicSymbol, defaultType)
    {
        Data = data;
    }

    public RawIntegerLiteralData Data { get; }

    public bool Negated { get; init; }
}

internal sealed record BoundUnfixedFloatLiteral : BoundSpeculativeExpression
{
    public BoundUnfixedFloatLiteral(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        RawFloatLiteralData data,
        TypeSymbol defaultType
    )
        : base(semanticContext, syntax, UnfixedFloatTypeSymbol.Instance.PublicSymbol, defaultType)
    {
        Data = data;
    }

    public RawFloatLiteralData Data { get; }

    public bool Negated { get; init; }
}

internal sealed record BoundLiteral : BoundExpression
{
    public BoundLiteral(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        ConstantValue value
    )
        : base(semanticContext, syntax, type)
    {
        Value = value;
    }

    public ConstantValue Value { get; }

    protected override ConstantValue? ComputeConstantValue() => Value;
}

internal sealed record BoundThisExpression : BoundExpression
{
    public BoundThisExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type
    )
        : base(semanticContext, syntax, type) { }
}

internal sealed record BoundTypeSize : BoundExpression
{
    public BoundTypeSize(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol targetType
    )
        : base(semanticContext, syntax, semanticContext.GetSpecialType(SpecialType.USize))
    {
        TargetType = targetType;
    }

    public TypeSymbol TargetType { get; }

    protected override ConstantValue? ComputeConstantValue()
    {
        if (TargetType.IsDynamicallySized)
            return null;

        var layout = SemanticContext.GetTypeLayout(TargetType);
        return SemanticContext.PointerWidth switch
        {
            PointerWidth.X32 => new IntegerConstant((uint)layout.Size),
            PointerWidth.X64 => new IntegerConstant(layout.Size),
            _ => throw new InvalidOperationException("Invalid pointer width"),
        };
    }
}

internal sealed record BoundVariableAccess : BoundExpression
{
    public BoundVariableAccess(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        VariableSymbol symbol
    )
        : base(semanticContext, syntax, symbol.Type)
    {
        Symbol = symbol;
    }

    public BoundVariableAccess(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression? owner,
        VariableSymbol symbol
    )
        : base(semanticContext, syntax, symbol.Type)
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
    public BoundParameterAccess(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        ParameterSymbol symbol
    )
        : base(semanticContext, syntax, symbol.Type)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        BoundExpression operand,
        UnaryOperation operation
    )
        : base(semanticContext, syntax, type)
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
            UnaryOperation.Negation => Operand.ConstantValue?.TryNegate(),
            UnaryOperation.LogicalNot => Operand.ConstantValue?.TryLogicalNot(),
            UnaryOperation.BitwiseNot => Operand.ConstantValue?.TryBitwiseNot(),
            _ => null,
        };
    }
}

internal sealed record BoundBinaryOperation : BoundExpression
{
    public BoundBinaryOperation(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        BoundExpression left,
        BoundExpression right,
        BinaryOperation operation
    )
        : base(semanticContext, syntax, type)
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

        return leftConst.TryBinary(Operation.ToConstantOperation(), in rightConst);
    }
}

internal sealed record BoundAssignmentOperation : BoundExpression
{
    public BoundAssignmentOperation(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        BoundExpression left,
        BoundExpression right,
        AssignmentOperation operation
    )
        : base(semanticContext, syntax, type)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        BoundExpression condition,
        BoundExpression whenTrue,
        BoundExpression whenFalse
    )
        : base(
            semanticContext,
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        BoundExpression condition,
        BoundExpression whenTrue,
        BoundExpression whenFalse
    )
        : base(semanticContext, syntax, type)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        FunctionSymbol function,
        BoundExpression? receiver,
        ImmutableArray<BoundExpression> arguments
    )
        : base(semanticContext, syntax, function.ReturnType)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        BoundExpression operand,
        Conversion conversion
    )
        : base(semanticContext, syntax, type)
    {
        Operand = operand;
        Conversion = conversion;
    }

    public BoundExpression Operand { get; }

    public Conversion Conversion { get; }

    protected override ConstantValue? ComputeConstantValue()
    {
        return Operand.ConstantValue?.TryConvert(Type, SemanticContext.PointerWidth);
    }
}

internal sealed record BoundAddressOf : BoundExpression
{
    public BoundAddressOf(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression operand,
        TypeSymbol type,
        bool isMutable
    )
        : base(semanticContext, syntax, type)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression operand,
        TypeSymbol type,
        bool isMutable
    )
        : base(semanticContext, syntax, type)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        BoundExpression operand,
        BoundExpression index,
        TypeSymbol type
    )
        : base(semanticContext, syntax, type)
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
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type,
        ImmutableArray<BoundExpression> expressions
    )
        : base(semanticContext, syntax, type)
    {
        Expressions = expressions;
    }

    public ImmutableArray<BoundExpression> Expressions { get; }
}

internal sealed record BoundSpeculativeCollectionExpression : BoundSpeculativeExpression
{
    public BoundSpeculativeCollectionExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        ImmutableArray<BoundExpression> expressions,
        TypeSymbol defaultType
    )
        : base(
            semanticContext,
            syntax,
            UndeterminedCollectionTypeSymbol.Instance.PublicSymbol,
            defaultType
        )
    {
        Expressions = expressions;
    }

    public ImmutableArray<BoundExpression> Expressions { get; }
}

internal readonly record struct BoundFieldInitializer(VariableSymbol Field, BoundExpression Value);

internal sealed record BoundNewExpression : BoundExpression
{
    public BoundNewExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol targetType,
        FunctionSymbol constructor,
        ImmutableArray<BoundExpression> arguments,
        ImmutableArray<BoundFieldInitializer> initializers = default
    )
        : base(semanticContext, syntax, targetType)
    {
        Constructor = constructor;
        Arguments = arguments;
        Initializers = initializers.NullToEmpty();
    }

    public FunctionSymbol Constructor { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }
    public ImmutableArray<BoundFieldInitializer> Initializers { get; }
}

internal sealed record BoundDefaultExpression : BoundExpression
{
    public BoundDefaultExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type
    )
        : base(semanticContext, syntax, type) { }

    protected override ConstantValue? ComputeConstantValue()
    {
        return Type switch
        {
            NullableTypeSymbol => new NullConstant(),
            NamedTypeSymbol
            {
                IsEnum: false,
                SpecialType: { IsNumeric: false, IsCharacter: false }
            } => null,
            NamedTypeSymbol type => CreateEnumOrNumericConstant(type),
            _ => (ConstantValue?)null,
        };

        ConstantValue CreateEnumOrNumericConstant(NamedTypeSymbol type)
        {
            var specialType = type.IsEnum ? type.EnumUnderlyingType.SpecialType : type.SpecialType;
            return Constants.ConstantValue.Zero(specialType, SemanticContext.PointerWidth);
        }
    }
}

internal sealed record BoundSpeculativeDefaultExpression : BoundSpeculativeExpression
{
    public BoundSpeculativeDefaultExpression(
        IBoundSemanticContext semanticContext,
        IBoundSource syntax,
        TypeSymbol type
    )
        : base(semanticContext, syntax, type, UndeterminedObjectTypeSymbol.Instance.PublicSymbol)
    { }
}
