// @file MetadataBuilderMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Metadata.Builders;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.BoundTree.Builders;

[Mapper(UseReferenceHandling = true)]
[UseStaticMapper(typeof(DescriptorMapper))]
[UseStaticMapper(typeof(ParameterDefinition.Mapper))]
internal static partial class MetadataBuilderMapper
{
    [MapDerivedType<MetadataConstantExpressionBuilder, MetadataConstantExpression>]
    [MapDerivedType<ThisMetadataExpressionBuilder, MetadataThisExpression>]
    [MapDerivedType<MetadataVariableAccessBuilder, MetadataVariableAccess>]
    [MapDerivedType<MetadataParameterAccessBuilder, MetadataParameterAccess>]
    [MapDerivedType<MetadataUnaryOperationBuilder, MetadataUnaryOperation>]
    [MapDerivedType<MetadataBinaryOperationBuilder, MetadataBinaryOperation>]
    [MapDerivedType<MetadataAssignmentOperationBuilder, MetadataAssignmentOperation>]
    [MapDerivedType<MetadataConditionalBuilder, MetadataConditional>]
    [MapDerivedType<MetadataInvocationBuilder, MetadataInvocation>]
    [MapDerivedType<MetadataConversionBuilder, MetadataConversion>]
    [MapDerivedType<MetadataAddressOfBuilder, MetadataAddressOf>]
    [MapDerivedType<MetadataDereferenceBuilder, MetadataDereference>]
    [MapDerivedType<MetadataIndexBuilder, MetadataIndex>]
    [MapDerivedType<MetadataCollectionExpressionBuilder, MetadataCollectionExpression>]
    [MapDerivedType<MetadataNewExpressionBuilder, MetadataNewExpression>]
    [MapDerivedType<MetadataDefaultExpressionBuilder, MetadataDefaultExpression>]
    public static partial MetadataExpression Map(
        MetadataExpressionBuilder builder,
        [ReferenceHandler] IReferenceHandler referenceHandler
    );
}
