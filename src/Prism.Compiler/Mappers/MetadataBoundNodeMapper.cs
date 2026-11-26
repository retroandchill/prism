using Prism.BoundTree;
using Prism.Compiler.Symbols.Metadata;
using Prism.Metadata.BoundTree;
using Prism.Metadata.Symbols;
using Prism.Semantic.Conversions;
using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Compiler.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class MetadataBoundNodeMapper(MetadataAssemblySymbol assemblySymbol)
{
    [MapValue(nameof(BoundNode.SemanticContext), Use = nameof(GetSemanticContext))]
    [MapProperty(nameof(MetadataExpression.Location), nameof(BoundNode.Syntax))]
    [MapValue(nameof(BoundNode.HasErrors), false)]
    [MapValue(nameof(BoundNode.WasCompilerGenerated), false)]
    [MapperIgnoreTarget(nameof(BoundNode.Location))]
    [MapDerivedType<MetadataConstantExpression, BoundLiteral>]
    [MapDerivedType<MetadataThisExpression, BoundThisExpression>]
    [MapDerivedType<MetadataVariableAccess, BoundVariableAccess>]
    [MapDerivedType<MetadataParameterAccess, BoundParameterAccess>]
    [MapDerivedType<MetadataUnaryOperation, BoundUnaryOperation>]
    [MapDerivedType<MetadataBinaryOperation, BoundBinaryOperation>]
    [MapDerivedType<MetadataAssignmentOperation, BoundAssignmentOperation>]
    [MapDerivedType<MetadataConditional, BoundConditional>]
    [MapDerivedType<MetadataInvocation, BoundInvocation>]
    [MapDerivedType<MetadataConversion, BoundConversion>]
    [MapDerivedType<MetadataAddressOf, BoundAddressOf>]
    [MapDerivedType<MetadataDereference, BoundDereference>]
    [MapDerivedType<MetadataIndex, BoundIndex>]
    [MapDerivedType<MetadataCollectionExpression, BoundCollectionExpression>]
    [MapDerivedType<MetadataNewExpression, BoundNewExpression>]
    [MapDerivedType<MetadataDefaultExpression, BoundDefaultExpression>]
    public partial BoundExpression Map(MetadataExpression expression);

    private partial BoundFieldInitializer Map(MetadataFieldInitializer expression);

#pragma warning disable CA1859
    private IBoundSemanticContext GetSemanticContext() => assemblySymbol;

    private static IBoundSource CreateSource(SourceLocation location) => new BoundSource(location);
#pragma warning restore CA1859

    private TypeSymbol MapType(TypeDescriptor typeDescriptor)
    {
        return assemblySymbol.GetReferencedType(typeDescriptor);
    }

    private VariableSymbol MapVariable(VariableDescriptor variable)
    {
        return assemblySymbol.GetReferencedVariable(variable);
    }

    private FunctionSymbol MapFunction(FunctionDescriptor function)
    {
        return assemblySymbol.GetReferencedFunction(function);
    }

    private ParameterSymbol MapParameter(ParameterDefinition parameter)
    {
        return assemblySymbol.GetOrAddParameter(parameter);
    }

    private static Conversion MapConversion(ConversionInfo conversionInfo)
    {
        return conversionInfo.Kind switch
        {
            ConversionInfoKind.NoConversion => Conversion.None,
            ConversionInfoKind.Identity => Conversion.Identity,
            ConversionInfoKind.ImplicitNumeric => Conversion.ImplicitNumeric,
            ConversionInfoKind.ImplicitCharacter => Conversion.ImplicitCharacter,
            ConversionInfoKind.ImplicitReference => Conversion.ImplicitReference,
            ConversionInfoKind.ImplicitSpan => Conversion.ImplicitSpan,
            ConversionInfoKind.ImplicitNullable => Conversion.ImplicitNullable([
                .. conversionInfo.UnderlyingConversions.Select(MapConversion),
            ]),
            ConversionInfoKind.ExplicitNumeric => Conversion.ExplicitNumeric,
            ConversionInfoKind.ExplicitCharacter => Conversion.ExplicitCharacter,
            ConversionInfoKind.ExplicitReference => Conversion.ExplicitReference,
            ConversionInfoKind.UnwrapNullable => Conversion.UnwrapNullable([
                .. conversionInfo.UnderlyingConversions.Select(MapConversion),
            ]),
            ConversionInfoKind.NullToNullable => Conversion.NullToNullable,
            _ => throw new ArgumentOutOfRangeException(nameof(conversionInfo)),
        };
    }

    private sealed class BoundSource(SourceLocation location) : IBoundSource
    {
        public SourceLocation Location { get; } = location;
        public bool HasErrors => false;
    }
}
