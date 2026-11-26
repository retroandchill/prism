// @file MetadataRowMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using Prism.Constants;
using Prism.Metadata.BoundTree;
using Prism.Metadata.BoundTree.Serialized;
using Prism.Metadata.Symbols;
using Prism.Utils;
using Riok.Mapperly.Abstractions;
using ZLinq;

namespace Prism.Metadata.Serialization;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal sealed partial class MetadataRowMapper(
    StringHeapBuilder strings,
    BlobHeapBuilder blobs,
    TypeSpecHeapBuilder typeSpecs
)
{
    private readonly TypeSpecHeapBuilder _typeSpecs = typeSpecs;

    private readonly Dictionary<ConstantValue, ConstantHandle> _constants = [];
    private readonly Dictionary<TypeSpecification, TypeSpecHandle> _typeSpecCache = [];

    public partial AssemblyIdentityData Map(AssemblyIdentity identity);

    private static VersionData Map(Version version) => version;

    public partial ImmutableArray<AssemblyReferenceRow> Map(
        ImmutableArray<AssemblyReference> references
    );

    private partial AssemblyReferenceRow Map(AssemblyReference reference);

    public partial ImmutableArray<NamespaceDefRow> Map(
        ImmutableArray<NamespaceDefinition> namespaces
    );

    [MapProperty(nameof(NamespaceDefinition.ParentHandle), nameof(NamespaceDefRow.Parent))]
    private partial NamespaceDefRow Map(NamespaceDefinition definition);

    public partial ImmutableArray<NamespaceRefRow> Map(
        ImmutableArray<NamespaceReference> namespaces
    );

    [MapProperty(nameof(NamespaceReference.ParentHandle), nameof(NamespaceRefRow.Parent))]
    private partial NamespaceRefRow Map(NamespaceReference reference);

    public partial ImmutableArray<TypeDefinitionRow> Map(ImmutableArray<TypeDefinition> namespaces);

    [MapProperty(nameof(TypeDefinition.ParentHandle), nameof(TypeDefinitionRow.Container))]
    [MapProperty(nameof(TypeDefinition.BaseType), nameof(TypeDefinitionRow.Base))]
    private partial TypeDefinitionRow Map(TypeDefinition definition);

    public partial ImmutableArray<TypeReferenceRow> Map(ImmutableArray<TypeReference> namespaces);

    [MapProperty(nameof(TypeReference.ParentHandle), nameof(TypeReferenceRow.Container))]
    private partial TypeReferenceRow Map(TypeReference reference);

    [MapDerivedType<ArrayTypeSpecification, ArrayTypeRow>]
    [MapDerivedType<ReferenceTypeSpecification, ReferenceTypeRow>]
    [MapDerivedType<NullableTypeSpecification, NullableTypeRow>]
    private partial TypeSpecRow Map(TypeSpecification specification);

    public partial ImmutableArray<TypeAliasRow> Map(ImmutableArray<TypeAlias> aliases);

    [MapProperty(nameof(TypeReference.ParentHandle), nameof(TypeReferenceRow.Container))]
    private partial TypeAliasRow Map(TypeAlias alias);

    private partial ArrayTypeRow Map(ArrayTypeSpecification specification);

    private partial ReferenceTypeRow Map(ReferenceTypeSpecification specification);

    private partial NullableTypeRow Map(NullableTypeSpecification specification);

    private TypeHandle MapType(TypeDescriptor descriptor)
    {
        return descriptor switch
        {
            TypeDefinition typeDef => typeDef.Handle,
            TypeReference typeRef => typeRef.Handle,
            TypeSpecification typeSpec => _typeSpecCache.GetOrAdd(
                typeSpec,
                static (typeSpec, @this) =>
                {
                    var row = @this.Map(typeSpec);
                    return @this._typeSpecs.Add(row);
                },
                this
            ),
            TypeAlias typeAlias => typeAlias.Handle,
        };
    }

    private TypeHandle MapType(TypeDescriptor? descriptor)
    {
        return descriptor switch
        {
            null => TypeHandle.None,
            { } desc => MapType(desc),
        };
    }

    public partial ImmutableArray<VariableRow> Map(ImmutableArray<VariableDefinition> namespaces);

    [MapProperty(nameof(TypeDefinition.ParentHandle), nameof(TypeDefinitionRow.Container))]
    private partial VariableRow Map(VariableDefinition variable);

    public partial ImmutableArray<FunctionRow> Map(ImmutableArray<FunctionDefinition> namespaces);

    [MapProperty(nameof(FunctionDefinition.ParentHandle), nameof(FunctionRow.Container))]
    private partial FunctionRow Map(FunctionDefinition function);

    public partial ImmutableArray<ParameterRow> Map(ImmutableArray<ParameterDefinition> namespaces);

    [MapProperty(
        nameof(ParameterDefinition.ParentHandle),
        nameof(ParameterRow.FunctionOrAttribute)
    )]
    [MapProperty(nameof(ParameterDefinition.DefaultValueHandle), nameof(ParameterRow.DefaultValue))]
    private partial ParameterRow Map(ParameterDefinition function);

    [MapProperty(nameof(EnumOrdinalDefinition.ParentHandle), nameof(EnumOrdinalRow.Parent))]
    public partial ImmutableArray<EnumOrdinalRow> Map(
        ImmutableArray<EnumOrdinalDefinition> ordinals
    );

    [MapProperty(nameof(EnumOrdinalDefinition.ParentHandle), nameof(EnumOrdinalRow.Parent))]
    private partial EnumOrdinalRow Map(EnumOrdinalDefinition ordinal);

    public partial ImmutableArray<CustomAttributeRow> Map(
        ImmutableArray<CustomAttributeDefinition> namespaces
    );

    [MapProperty(nameof(CustomAttributeDefinition.ParentHandle), nameof(CustomAttributeRow.Holder))]
    [MapProperty(nameof(CustomAttributeDefinition.Type), nameof(CustomAttributeRow.Type))]
    private partial CustomAttributeRow Map(CustomAttributeDefinition definition);

    public partial ImmutableArray<VariableReferenceRow> Map(
        ImmutableArray<VariableReference> variables
    );

    [MapProperty(nameof(VariableReference.ParentHandle), nameof(VariableReferenceRow.Container))]
    private partial VariableReferenceRow Map(VariableReference variable);

    public partial ImmutableArray<FunctionReferenceRow> Map(
        ImmutableArray<FunctionReference> variables
    );

    [MapProperty(nameof(FunctionReference.ParentHandle), nameof(FunctionReferenceRow.Container))]
    private partial FunctionReferenceRow Map(FunctionReference function);

    private BlobHandle StoreParameterTypes(ImmutableArray<TypeDescriptor> parameters)
    {
        const int maxParameterCount = 100;
        TypeHandle[]? rentedArray = null;
        try
        {
            Span<TypeHandle> typeHandles = stackalloc TypeHandle[maxParameterCount];
            if (parameters.Length > maxParameterCount)
            {
                rentedArray = ArrayPool<TypeHandle>.Shared.Rent(parameters.Length);
                typeHandles = rentedArray;
            }

            typeHandles = typeHandles[..parameters.Length];

            foreach (var (i, type) in parameters.AsValueEnumerable().Index())
            {
                typeHandles[i] = MapType(type);
            }

            return blobs.Add(typeHandles);
        }
        finally
        {
            if (rentedArray is not null)
            {
                ArrayPool<TypeHandle>.Shared.Return(rentedArray);
            }
        }
    }

    private SourceLocationData Map(SourceLocation location)
    {
        return new SourceLocationData(
            MapString(location.SyntaxTree.Path ?? ""),
            location.SourceSpan
        );
    }

    private static VariableDescriptorHandle MapToHandle(VariableDescriptor descriptor)
    {
        return descriptor.Handle;
    }

    private static FunctionDescriptorHandle MapToHandle(FunctionDescriptor descriptor)
    {
        return descriptor.Handle;
    }

    private static ParameterHandle MapToHandle(ParameterDefinition descriptor)
    {
        return descriptor.Handle;
    }

    private partial ConversionInfoData Map(ConversionInfo info);

    [MapDerivedType<ConversionInfo.NestedData, ConversionInfoData.NestedData>]
    private partial ConversionInfoData.SpecialData Map(ConversionInfo.SpecialData data);

    [MapDerivedType<MetadataConstantExpression, MetadataConstantExpressionEntry>]
    [MapDerivedType<MetadataThisExpression, ThisMetadataExpressionEntry>]
    [MapDerivedType<MetadataVariableAccess, MetadataVariableAccessEntry>]
    [MapDerivedType<MetadataParameterAccess, MetadataParameterAccessEntry>]
    [MapDerivedType<MetadataUnaryOperation, MetadataUnaryOperationEntry>]
    [MapDerivedType<MetadataBinaryOperation, MetadataBinaryOperationEntry>]
    [MapDerivedType<MetadataAssignmentOperation, MetadataAssignmentOperationEntry>]
    [MapDerivedType<MetadataConditional, MetadataConditionalEntry>]
    [MapDerivedType<MetadataInvocation, MetadataInvocationEntry>]
    [MapDerivedType<MetadataConversion, MetadataConversionEntry>]
    [MapDerivedType<MetadataAddressOf, MetadataAddressOfEntry>]
    [MapDerivedType<MetadataDereference, MetadataDereferenceEntry>]
    [MapDerivedType<MetadataIndex, MetadataIndexEntry>]
    [MapDerivedType<MetadataCollectionExpression, MetadataCollectionExpressionEntry>]
    [MapDerivedType<MetadataNewExpression, MetadataNewExpressionEntry>]
    [MapDerivedType<MetadataDefaultExpression, MetadataDefaultExpressionEntry>]
    public partial MetadataExpressionEntry Map(MetadataExpression expression);

    private BlobHandle MapConstantArray(ImmutableArray<ConstantValue> arguments)
    {
        if (arguments.IsEmpty)
            return BlobHandle.None;

        const int maxStackArgs = 32;
        Span<ConstantHandle> stackArgs = stackalloc ConstantHandle[maxStackArgs];

        ConstantHandle[]? rentedStackArgs = null;
        try
        {
            if (arguments.Length > maxStackArgs)
            {
                rentedStackArgs = ArrayPool<ConstantHandle>.Shared.Rent(arguments.Length);
            }

            var args = rentedStackArgs is not null
                ? rentedStackArgs.AsSpan(0, arguments.Length)
                : stackArgs[..arguments.Length];
            foreach (var (i, arg) in arguments.AsValueEnumerable().Index())
            {
                args[i] = MapConstant(arg);
            }

            return blobs.Add(args);
        }
        finally
        {
            if (rentedStackArgs is not null)
                ArrayPool<ConstantHandle>.Shared.Return(rentedStackArgs);
        }
    }

    private StringHandle MapString(string str)
    {
        return strings.GetOrAdd(str);
    }

    private ConstantHandle MapConstant(ConstantValue? value)
    {
        return value is not { } constantValue ? ConstantHandle.None : MapConstant(constantValue);
    }

    private ConstantHandle MapConstant(ConstantValue value)
    {
        return _constants.GetOrAdd(
            value,
            static (constant, @this) =>
            {
                return constant switch
                {
                    string str => @this.MapString(str),
                    _ => @this.AddNonStringConstant(in constant),
                };
            },
            this
        );
    }

    [UserMapping(Ignore = true)]
    private ConstantHandle AddNonStringConstant(in ConstantValue value)
    {
        return InlineConstHandle.TryCreate(in value)?.ToConstantHandle() ?? blobs.Add(in value);
    }
}
