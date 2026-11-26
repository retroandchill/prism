using Prism.Constants;
using Prism.Metadata.BoundTree;
using Prism.Metadata.BoundTree.Serialized;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Metadata.Serialization;

[Mapper]
internal sealed partial class MetadataBoundTreeReader(MetadataReader reader)
{
    [MapDerivedType<MetadataConstantExpressionEntry, MetadataConstantExpression>]
    [MapDerivedType<ThisMetadataExpressionEntry, MetadataThisExpression>]
    [MapDerivedType<MetadataVariableAccessEntry, MetadataVariableAccess>]
    [MapDerivedType<MetadataParameterAccessEntry, MetadataParameterAccess>]
    [MapDerivedType<MetadataUnaryOperationEntry, MetadataUnaryOperation>]
    [MapDerivedType<MetadataBinaryOperationEntry, MetadataBinaryOperation>]
    [MapDerivedType<MetadataAssignmentOperationEntry, MetadataAssignmentOperation>]
    [MapDerivedType<MetadataConditionalEntry, MetadataConditional>]
    [MapDerivedType<MetadataInvocationEntry, MetadataInvocation>]
    [MapDerivedType<MetadataConversionEntry, MetadataConversion>]
    [MapDerivedType<MetadataAddressOfEntry, MetadataAddressOf>]
    [MapDerivedType<MetadataDereferenceEntry, MetadataDereference>]
    [MapDerivedType<MetadataIndexEntry, MetadataIndex>]
    [MapDerivedType<MetadataCollectionExpressionEntry, MetadataCollectionExpression>]
    [MapDerivedType<MetadataNewExpressionEntry, MetadataNewExpression>]
    [MapDerivedType<MetadataDefaultExpressionEntry, MetadataDefaultExpression>]
    public partial MetadataExpression Map(MetadataExpressionEntry entry);

    private SourceLocation MapLocation(SourceLocationData data)
    {
        return new SourceLocation(GetSourceDocument(data.File), data.SourceSpan);
    }

    private ISourceDocument GetSourceDocument(StringHandle handle)
    {
        // TODO: We need to create our source document implementation
        var path = reader.GetString(handle);
        return null!;
    }

    private ConstantValue GetConstantValue(ConstantHandle handle)
    {
        return reader.GetConstant(handle);
    }

    private TypeDescriptor GetTypeDescriptor(TypeHandle handle)
    {
        return reader.GetTypeDescriptor(handle);
    }

    private VariableDescriptor GetVariableDescriptor(VariableDescriptorHandle handle)
    {
        return reader.GetVariableDescriptor(handle);
    }

    private FunctionDescriptor GetFunctionDescriptor(FunctionDescriptorHandle handle)
    {
        return reader.GetFunctionDescriptor(handle);
    }

    private ParameterDefinition GetParameter(ParameterHandle handle)
    {
        return reader.GetParameter(handle);
    }

    private partial ConversionInfo Map(ConversionInfoData info);

    [MapDerivedType<ConversionInfoData.NestedData, ConversionInfo.NestedData>]
    private partial ConversionInfo.SpecialData Map(ConversionInfoData.SpecialData data);
}
