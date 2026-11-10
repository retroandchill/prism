// @file MetadataRowMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using Prism.Utils;
using Riok.Mapperly.Abstractions;
using ZLinq;
using ConstantValue = Prism.Constants.ConstantValue;

namespace Prism.Metadata;

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
    [MapProperty(nameof(TypeDefinition.BaseTypeHandle), nameof(TypeDefinitionRow.Base))]
    private partial TypeDefinitionRow Map(TypeDefinition definition);

    public partial ImmutableArray<TypeReferenceRow> Map(ImmutableArray<TypeReference> namespaces);

    [MapProperty(nameof(TypeReference.ParentHandle), nameof(TypeReferenceRow.Container))]
    private partial TypeReferenceRow Map(TypeReference reference);

    [MapDerivedType<ArrayTypeSpecification, ArrayTypeRow>]
    [MapDerivedType<ReferenceTypeSpecification, ReferenceTypeRow>]
    [MapDerivedType<NullableTypeSpecification, NullableTypeRow>]
    public partial TypeSpecRow Map(TypeSpecification specification);

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

    public partial ImmutableArray<CustomAttributeRow> Map(
        ImmutableArray<CustomAttributeDefinition> namespaces
    );

    [MapProperty(nameof(CustomAttributeDefinition.ParentHandle), nameof(CustomAttributeRow.Holder))]
    [MapProperty(nameof(CustomAttributeDefinition.TypeHandle), nameof(CustomAttributeRow.Type))]
    private partial CustomAttributeRow Map(CustomAttributeDefinition definition);

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
        if (value is not { } constantValue)
            return ConstantHandle.None;

        return _constants.GetOrAdd(
            constantValue,
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
