// @file MetadataExpression.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Constants;
using Prism.Metadata.Builders;

namespace Prism.Metadata.BoundTree.Builders;

public closed class MetadataExpressionBuilder : MetadataNodeBuilder
{
    private protected MetadataExpressionBuilder(SourceLocation location, TypeDescriptorBuilder type)
        : base(location)
    {
        Type = type;
    }

    public TypeDescriptorBuilder Type { get; }
}

public sealed class MetadataConstantExpressionBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    ConstantValue value
) : MetadataExpressionBuilder(location, type)
{
    public ConstantValue Value { get; } = value;
}

public sealed class ThisMetadataExpressionBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type
) : MetadataExpressionBuilder(location, type);

public sealed class MetadataVariableAccessBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder? owner,
    VariableDescriptorBuilder symbol
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder? Owner { get; } = owner;

    public VariableDescriptorBuilder Symbol { get; } = symbol;
}

public sealed class MetadataParameterAccessBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    ParameterDefinitionBuilder symbol
) : MetadataExpressionBuilder(location, type)
{
    public ParameterDefinitionBuilder Symbol { get; } = symbol;
}

public sealed class MetadataUnaryOperationBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder operand,
    UnaryOperation operation
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Operand { get; } = operand;

    public UnaryOperation Operation { get; } = operation;
}

public sealed class MetadataBinaryOperationBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder left,
    MetadataExpressionBuilder right,
    BinaryOperation operation
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Left { get; } = left;

    public MetadataExpressionBuilder Right { get; } = right;

    public BinaryOperation Operation { get; } = operation;
}

public sealed class MetadataAssignmentOperationBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder left,
    MetadataExpressionBuilder right,
    AssignmentOperation operation
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Left { get; } = left;

    public MetadataExpressionBuilder Right { get; } = right;

    public AssignmentOperation Operation { get; } = operation;
}

public sealed class MetadataConditionalBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder condition,
    MetadataExpressionBuilder whenTrue,
    MetadataExpressionBuilder whenFalse
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Condition { get; } = condition;

    public MetadataExpressionBuilder WhenTrue { get; } = whenTrue;

    public MetadataExpressionBuilder WhenFalse { get; } = whenFalse;
}

public sealed class MetadataInvocationBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    FunctionDescriptorBuilder function,
    MetadataExpressionBuilder? receiver,
    ImmutableArray<MetadataExpressionBuilder> arguments
) : MetadataExpressionBuilder(location, type)
{
    public FunctionDescriptorBuilder Function { get; } = function;

    public MetadataExpressionBuilder? Receiver { get; } = receiver;

    public ImmutableArray<MetadataExpressionBuilder> Arguments { get; } = arguments;
}

public sealed class MetadataConversionBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder operand,
    ConversionInfo conversion
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Operand { get; } = operand;

    public ConversionInfo Conversion { get; } = conversion;
}

public sealed class MetadataAddressOfBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder operand,
    bool isMutable
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Operand { get; } = operand;

    public bool IsMutable { get; } = isMutable;
}

public sealed class MetadataDereferenceBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder operand,
    bool isAssignable
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Operand { get; } = operand;

    public bool IsAssignable { get; } = isAssignable;
}

public sealed class MetadataIndexBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    MetadataExpressionBuilder operand,
    MetadataExpressionBuilder index
) : MetadataExpressionBuilder(location, type)
{
    public MetadataExpressionBuilder Operand { get; } = operand;

    public MetadataExpressionBuilder Index { get; } = index;
}

public sealed class MetadataCollectionExpressionBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    ImmutableArray<MetadataExpressionBuilder> expressions
) : MetadataExpressionBuilder(location, type)
{
    public ImmutableArray<MetadataExpressionBuilder> Expressions { get; } = expressions;
}

public readonly record struct MetadataFieldInitializerBuilder(
    VariableDescriptorBuilder Field,
    MetadataExpressionBuilder Value
);

public sealed class MetadataNewExpressionBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type,
    FunctionDescriptorBuilder constructor,
    ImmutableArray<MetadataExpressionBuilder> arguments,
    ImmutableArray<MetadataFieldInitializerBuilder> initializers
) : MetadataExpressionBuilder(location, type)
{
    public FunctionDescriptorBuilder Constructor { get; } = constructor;

    public ImmutableArray<MetadataExpressionBuilder> Arguments { get; } = arguments;

    public ImmutableArray<MetadataFieldInitializerBuilder> Initializers { get; } = initializers;
}

public sealed class MetadataDefaultExpressionBuilder(
    SourceLocation location,
    TypeDescriptorBuilder type
) : MetadataExpressionBuilder(location, type);
