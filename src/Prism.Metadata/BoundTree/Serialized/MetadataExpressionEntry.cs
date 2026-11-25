// @file MetadataExpressionEntry.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using MagicArchive;

namespace Prism.Metadata.BoundTree.Serialized;

[Archivable]
[ArchivableUnion(ExpressionOffset + 1, typeof(MetadataConstantExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 2, typeof(ThisMetadataExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 3, typeof(MetadataVariableAccessEntry))]
[ArchivableUnion(ExpressionOffset + 4, typeof(MetadataParameterAccessEntry))]
[ArchivableUnion(ExpressionOffset + 5, typeof(MetadataUnaryOperationEntry))]
[ArchivableUnion(ExpressionOffset + 6, typeof(MetadataBinaryOperationEntry))]
[ArchivableUnion(ExpressionOffset + 7, typeof(MetadataAssignmentOperationEntry))]
[ArchivableUnion(ExpressionOffset + 8, typeof(MetadataConditionalEntry))]
[ArchivableUnion(ExpressionOffset + 9, typeof(MetadataInvocationEntry))]
[ArchivableUnion(ExpressionOffset + 10, typeof(MetadataConversionEntry))]
[ArchivableUnion(ExpressionOffset + 11, typeof(MetadataAddressOfEntry))]
[ArchivableUnion(ExpressionOffset + 12, typeof(MetadataDereferenceEntry))]
[ArchivableUnion(ExpressionOffset + 13, typeof(MetadataIndexEntry))]
[ArchivableUnion(ExpressionOffset + 14, typeof(MetadataCollectionExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 15, typeof(MetadataNewExpressionEntry))]
[ArchivableUnion(ExpressionOffset + 16, typeof(MetadataDefaultExpressionEntry))]
internal closed partial class MetadataExpressionEntry : MetadataNodeEntry
{
    private protected MetadataExpressionEntry(SourceLocationData location, TypeHandle type)
        : base(location)
    {
        Type = type;
    }

    public TypeHandle Type { get; }
}

[Archivable]
internal sealed partial class MetadataConstantExpressionEntry : MetadataExpressionEntry
{
    internal MetadataConstantExpressionEntry(
        SourceLocationData location,
        TypeHandle type,
        ConstantHandle value
    )
        : base(location, type)
    {
        Value = value;
    }

    public ConstantHandle Value { get; }
}

[Archivable]
internal sealed partial class ThisMetadataExpressionEntry : MetadataExpressionEntry
{
    internal ThisMetadataExpressionEntry(SourceLocationData location, TypeHandle type)
        : base(location, type) { }
}

[Archivable]
internal sealed partial class MetadataVariableAccessEntry : MetadataExpressionEntry
{
    internal MetadataVariableAccessEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry? owner,
        VariableDescriptorHandle symbol
    )
        : base(location, type)
    {
        Owner = owner;
        Symbol = symbol;
    }

    public MetadataExpressionEntry? Owner { get; }

    public VariableDescriptorHandle Symbol { get; }
}

[Archivable]
internal sealed partial class MetadataParameterAccessEntry : MetadataExpressionEntry
{
    internal MetadataParameterAccessEntry(
        SourceLocationData location,
        TypeHandle type,
        ParameterHandle symbol
    )
        : base(location, type)
    {
        Symbol = symbol;
    }

    public ParameterHandle Symbol { get; }
}

[Archivable]
internal sealed partial class MetadataUnaryOperationEntry : MetadataExpressionEntry
{
    internal MetadataUnaryOperationEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry operand,
        UnaryOperation operation
    )
        : base(location, type)
    {
        Operand = operand;
        Operation = operation;
    }

    public MetadataExpressionEntry Operand { get; }

    public UnaryOperation Operation { get; }
}

[Archivable]
internal sealed partial class MetadataBinaryOperationEntry : MetadataExpressionEntry
{
    internal MetadataBinaryOperationEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry left,
        MetadataExpressionEntry right,
        BinaryOperation operation
    )
        : base(location, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public MetadataExpressionEntry Left { get; }

    public MetadataExpressionEntry Right { get; }

    public BinaryOperation Operation { get; }
}

[Archivable]
internal sealed partial class MetadataAssignmentOperationEntry : MetadataExpressionEntry
{
    internal MetadataAssignmentOperationEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry left,
        MetadataExpressionEntry right,
        AssignmentOperation operation
    )
        : base(location, type)
    {
        Left = left;
        Right = right;
        Operation = operation;
    }

    public MetadataExpressionEntry Left { get; }

    public MetadataExpressionEntry Right { get; }

    public AssignmentOperation Operation { get; }
}

[Archivable]
internal sealed partial class MetadataConditionalEntry : MetadataExpressionEntry
{
    internal MetadataConditionalEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry condition,
        MetadataExpressionEntry whenTrue,
        MetadataExpressionEntry whenFalse
    )
        : base(location, type)
    {
        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
    }

    public MetadataExpressionEntry Condition { get; }

    public MetadataExpressionEntry WhenTrue { get; }

    public MetadataExpressionEntry WhenFalse { get; }
}

[Archivable]
internal sealed partial class MetadataInvocationEntry : MetadataExpressionEntry
{
    internal MetadataInvocationEntry(
        SourceLocationData location,
        TypeHandle type,
        FunctionDescriptorHandle function,
        MetadataExpressionEntry? receiver,
        ImmutableArray<MetadataExpressionEntry> arguments
    )
        : base(location, type)
    {
        Function = function;
        Receiver = receiver;
        Arguments = arguments;
    }

    public FunctionDescriptorHandle Function { get; }

    public MetadataExpressionEntry? Receiver { get; }

    public ImmutableArray<MetadataExpressionEntry> Arguments { get; }
}

[Archivable]
internal sealed partial class MetadataConversionEntry : MetadataExpressionEntry
{
    internal MetadataConversionEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry operand,
        ConversionInfoData conversion
    )
        : base(location, type)
    {
        Operand = operand;
        Conversion = conversion;
    }

    public MetadataExpressionEntry Operand { get; }

    public ConversionInfoData Conversion { get; }
}

[Archivable]
internal sealed partial class MetadataAddressOfEntry : MetadataExpressionEntry
{
    internal MetadataAddressOfEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry operand,
        bool isMutable
    )
        : base(location, type)
    {
        Operand = operand;
        IsMutable = isMutable;
    }

    public MetadataExpressionEntry Operand { get; }

    public bool IsMutable { get; }
}

[Archivable]
internal sealed partial class MetadataDereferenceEntry : MetadataExpressionEntry
{
    internal MetadataDereferenceEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry operand,
        bool isAssignable
    )
        : base(location, type)
    {
        Operand = operand;
        IsAssignable = isAssignable;
    }

    public MetadataExpressionEntry Operand { get; }

    public bool IsAssignable { get; }
}

[Archivable]
internal sealed partial class MetadataIndexEntry : MetadataExpressionEntry
{
    internal MetadataIndexEntry(
        SourceLocationData location,
        TypeHandle type,
        MetadataExpressionEntry operand,
        MetadataExpressionEntry index
    )
        : base(location, type)
    {
        Operand = operand;
        Index = index;
    }

    public MetadataExpressionEntry Operand { get; }

    public MetadataExpressionEntry Index { get; }
}

[Archivable]
internal sealed partial class MetadataCollectionExpressionEntry : MetadataExpressionEntry
{
    internal MetadataCollectionExpressionEntry(
        SourceLocationData location,
        TypeHandle type,
        ImmutableArray<MetadataExpressionEntry> expressions
    )
        : base(location, type)
    {
        Expressions = expressions;
    }

    public ImmutableArray<MetadataExpressionEntry> Expressions { get; }
}

[Archivable]
internal readonly partial record struct MetadataFieldInitializerData(
    VariableDescriptorHandle Field,
    MetadataExpressionEntry Value
);

[Archivable]
internal sealed partial class MetadataNewExpressionEntry : MetadataExpressionEntry
{
    internal MetadataNewExpressionEntry(
        SourceLocationData location,
        TypeHandle type,
        FunctionDescriptorHandle constructor,
        ImmutableArray<MetadataExpressionEntry> arguments,
        ImmutableArray<MetadataFieldInitializerData> initializers
    )
        : base(location, type)
    {
        Constructor = constructor;
        Arguments = arguments;
        Initializers = initializers;
    }

    public FunctionDescriptorHandle Constructor { get; }

    public ImmutableArray<MetadataExpressionEntry> Arguments { get; }

    public ImmutableArray<MetadataFieldInitializerData> Initializers { get; }
}

[Archivable]
internal sealed partial class MetadataDefaultExpressionEntry : MetadataExpressionEntry
{
    internal MetadataDefaultExpressionEntry(SourceLocationData location, TypeHandle type)
        : base(location, type) { }
}
