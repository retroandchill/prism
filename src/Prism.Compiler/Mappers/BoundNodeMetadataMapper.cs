// @file BoundNodeMetadataMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.BoundTree;
using Prism.Compiler.Codegen;
using Prism.Metadata.BoundTree;
using Prism.Metadata.BoundTree.Builders;
using Prism.Metadata.Builders;
using Prism.Semantic.Conversions;
using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Compiler.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class BoundNodeMetadataMapper(MetadataEmitter emitter)
{
    [UserMapping(Default = true)]
    public MetadataExpressionBuilder Map(BoundExpression expression)
    {
        if (expression.ConstantValue is { } constantValue)
        {
            return new MetadataConstantExpressionBuilder(
                expression.Location,
                MapType(expression.Type),
                constantValue
            );
        }

        return MapInternal(expression);
    }

    [MapDerivedType<BoundThisExpression, ThisMetadataExpressionBuilder>]
    [MapDerivedType<BoundVariableAccess, MetadataVariableAccessBuilder>]
    [MapDerivedType<BoundParameterAccess, MetadataParameterAccessBuilder>]
    [MapDerivedType<BoundUnaryOperation, MetadataUnaryOperationBuilder>]
    [MapDerivedType<BoundBinaryOperation, MetadataBinaryOperationBuilder>]
    [MapDerivedType<BoundAssignmentOperation, MetadataAssignmentOperationBuilder>]
    [MapDerivedType<BoundConditional, MetadataConditionalBuilder>]
    [MapDerivedType<BoundInvocation, MetadataInvocationBuilder>]
    [MapDerivedType<BoundConversion, MetadataConversionBuilder>]
    [MapDerivedType<BoundAddressOf, MetadataAddressOfBuilder>]
    [MapDerivedType<BoundDereference, MetadataDereferenceBuilder>]
    [MapDerivedType<BoundIndex, MetadataIndexBuilder>]
    [MapDerivedType<BoundCollectionExpression, MetadataCollectionExpressionBuilder>]
    [MapDerivedType<BoundNewExpression, MetadataNewExpressionBuilder>]
    [MapDerivedType<BoundDefaultExpression, MetadataDefaultExpressionBuilder>]
    private partial MetadataExpressionBuilder MapInternal(BoundExpression expression);

    private TypeDescriptorBuilder MapType(TypeSymbol typeSymbol)
    {
        return emitter.GetOrAddType(typeSymbol).Value;
    }

    private VariableDescriptorBuilder MapVariable(VariableSymbol variable)
    {
        return emitter.GetOrAddVariable(variable);
    }

    private FunctionDescriptorBuilder MapFunction(FunctionSymbol function)
    {
        return emitter.GetOrAddFunction(function);
    }

    private ParameterDefinitionBuilder MapParameter(ParameterSymbol parameter)
    {
        return emitter.GetParameter(parameter);
    }

    private static ConversionInfo MapConversion(Conversion conversion)
    {
        return conversion.Kind switch
        {
            ConversionKind.NoConversion => ConversionInfo.None,
            ConversionKind.Identity => ConversionInfo.Identity,
            ConversionKind.ImplicitNumeric => ConversionInfo.ImplicitNumeric,
            ConversionKind.ImplicitCharacter => ConversionInfo.ImplicitCharacter,
            ConversionKind.ImplicitReference => ConversionInfo.ImplicitReference,
            ConversionKind.ImplicitSpan => ConversionInfo.ImplicitSpan,
            ConversionKind.ImplicitNullable => ConversionInfo.ImplicitNullable([
                .. conversion.UnderlyingConversions.Select(MapConversion),
            ]),
            ConversionKind.ExplicitNumeric => ConversionInfo.ExplicitNumeric,
            ConversionKind.ExplicitCharacter => ConversionInfo.ExplicitCharacter,
            ConversionKind.ExplicitReference => ConversionInfo.ExplicitReference,
            ConversionKind.UnwrapNullable => ConversionInfo.UnwrapNullable([
                .. conversion.UnderlyingConversions.Select(MapConversion),
            ]),
            ConversionKind.NullToNullable => ConversionInfo.NullToNullable,
            _ => throw new ArgumentOutOfRangeException(nameof(conversion)),
        };
    }
}
