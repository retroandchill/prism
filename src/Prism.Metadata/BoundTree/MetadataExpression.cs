// @file MetadataExpression.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Linq.Expressions;
using Prism.Constants;
using Prism.Metadata.Symbols;

namespace Prism.Metadata.BoundTree;

public closed class MetadataExpression : MetadataNode
{
    private protected MetadataExpression(SourceLocation location, TypeDescriptor type)
        : base(location)
    {
        Type = type;
    }

    public TypeDescriptor Type { get; }
}

public sealed class MetadataConstantExpression : MetadataExpression
{
    internal MetadataConstantExpression(
        SourceLocation location,
        TypeDescriptor type,
        ConstantValue value
    )
        : base(location, type)
    {
        Value = value;
    }

    public ConstantValue Value { get; }
}

public sealed class MetadataThisExpression : MetadataExpression
{
    internal MetadataThisExpression(SourceLocation location, TypeDescriptor type)
        : base(location, type) { }
}

public sealed class MetadataVariableAccess : MetadataExpression
{
    internal MetadataVariableAccess(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression? owner,
        VariableDescriptor symbol
    )
        : base(location, type)
    {
        Owner = owner;
        Symbol = symbol;
    }

    public MetadataExpression? Owner { get; }

    public VariableDescriptor Symbol { get; }
}

public sealed class MetadataParameterAccess : MetadataExpression
{
    internal MetadataParameterAccess(
        SourceLocation location,
        TypeDescriptor type,
        ParameterDefinition symbol
    )
        : base(location, type)
    {
        Symbol = symbol;
    }

    public ParameterDefinition Symbol { get; }
}

public sealed class MetadataUnaryOperation : MetadataExpression
{
    internal MetadataUnaryOperation(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression operand,
        UnaryOperation operation
    )
        : base(location, type)
    {
        Operand = operand;
        Operation = operation;
    }

    public MetadataExpression Operand { get; }

    public UnaryOperation Operation { get; }
}

public sealed class MetadataBinaryOperation : MetadataExpression
{
    internal MetadataBinaryOperation(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression left,
        MetadataExpression right,
        BinaryOperation operation
    )
        : base(location, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public MetadataExpression Left { get; }

    public MetadataExpression Right { get; }

    public BinaryOperation Operation { get; }
}

public sealed class MetadataAssignmentOperation : MetadataExpression
{
    internal MetadataAssignmentOperation(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression left,
        MetadataExpression right,
        AssignmentOperation operation
    )
        : base(location, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public MetadataExpression Left { get; }

    public MetadataExpression Right { get; }

    public AssignmentOperation Operation { get; }
}

public sealed class MetadataConditional : MetadataExpression
{
    internal MetadataConditional(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression condition,
        MetadataExpression whenTrue,
        MetadataExpression whenFalse
    )
        : base(location, type)
    {
        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
    }

    public MetadataExpression Condition { get; }

    public MetadataExpression WhenTrue { get; }

    public MetadataExpression WhenFalse { get; }
}

public sealed class MetadataInvocation : MetadataExpression
{
    internal MetadataInvocation(
        SourceLocation location,
        TypeDescriptor type,
        FunctionDescriptor function,
        MetadataExpression? receiver,
        ImmutableArray<MetadataExpression> arguments
    )
        : base(location, type)
    {
        Function = function;
        Receiver = receiver;
        Arguments = arguments;
    }

    public FunctionDescriptor Function { get; }

    public MetadataExpression? Receiver { get; }

    public ImmutableArray<MetadataExpression> Arguments { get; }
}

public sealed class MetadataConversion : MetadataExpression
{
    internal MetadataConversion(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression operand,
        ConversionInfo conversion
    )
        : base(location, type)
    {
        Operand = operand;
        Conversion = conversion;
    }

    public MetadataExpression Operand { get; }

    public ConversionInfo Conversion { get; }
}

public sealed class MetadataAddressOf : MetadataExpression
{
    internal MetadataAddressOf(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression operand,
        bool isMutable
    )
        : base(location, type)
    {
        Operand = operand;
        IsMutable = isMutable;
    }

    public MetadataExpression Operand { get; }

    public bool IsMutable { get; }
}

public sealed class MetadataDereference : MetadataExpression
{
    internal MetadataDereference(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression operand,
        bool isAssignable
    )
        : base(location, type)
    {
        Operand = operand;
        IsAssignable = isAssignable;
    }

    public MetadataExpression Operand { get; }

    public bool IsAssignable { get; }
}

public sealed class MetadataIndex : MetadataExpression
{
    internal MetadataIndex(
        SourceLocation location,
        TypeDescriptor type,
        MetadataExpression operand,
        MetadataExpression index
    )
        : base(location, type)
    {
        Operand = operand;
        Index = index;
    }

    public MetadataExpression Operand { get; }

    public MetadataExpression Index { get; }
}

public sealed class MetadataCollectionExpression : MetadataExpression
{
    internal MetadataCollectionExpression(
        SourceLocation location,
        TypeDescriptor type,
        ImmutableArray<MetadataExpression> expressions
    )
        : base(location, type)
    {
        Expressions = expressions;
    }

    public ImmutableArray<MetadataExpression> Expressions { get; }
}

public readonly record struct MetadataFieldInitializer(
    VariableDescriptor Field,
    MetadataExpression Value
);

public sealed class MetadataNewExpression : MetadataExpression
{
    internal MetadataNewExpression(
        SourceLocation location,
        TypeDescriptor type,
        FunctionDescriptor constructor,
        ImmutableArray<MetadataExpression> arguments,
        ImmutableArray<MetadataFieldInitializer> initializers
    )
        : base(location, type)
    {
        Constructor = constructor;
        Arguments = arguments;
        Initializers = initializers;
    }

    public FunctionDescriptor Constructor { get; }

    public ImmutableArray<MetadataExpression> Arguments { get; }

    public ImmutableArray<MetadataFieldInitializer> Initializers { get; }
}

public sealed class MetadataDefaultExpression : MetadataExpression
{
    internal MetadataDefaultExpression(SourceLocation location, TypeDescriptor type)
        : base(location, type) { }
}
