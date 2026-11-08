// @file ParameterDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

public closed record ParameterDefaultDefinition;

public sealed record ConstantParameterDefaultDefinition(ConstantValue Value)
    : ParameterDefaultDefinition;

public union FunctionOrAttribute(TypeDefinition, FunctionDefinition)
{
    public readonly FunctionOrAttributeHandle Handle
    {
        get
        {
            return this switch
            {
                TypeDefinition def => def.Handle,
                FunctionDefinition def => def.Handle,
            };
        }
    }
}

public sealed partial class ParameterDefinition
{
    public ParameterHandle Handle { get; }
    private readonly TypeHandle _type;
    private readonly MetadataReader? _reader;

    internal ParameterDefinition(
        ParameterHandle handle,
        ParameterAttributes attributes,
        string name,
        FunctionOrAttributeHandle parent,
        TypeHandle type,
        ParameterDefaultHandle defaultValue,
        MetadataReader reader
    )
    {
        Handle = handle;
        _type = type;
        _reader = reader;
        Attributes = attributes;
        Name = name;
        ParentHandle = parent;
        DefaultValueHandle = defaultValue;
    }

    [MapperConstructor]
    private ParameterDefinition(
        ParameterHandle handle,
        ParameterAttributes attributes,
        string name,
        TypeDescriptor type,
        ParameterDefaultDefinition? defaultValue
    )
    {
        Handle = handle;
        Type = type;
        Attributes = attributes;
        Name = name;
        DefaultValue = defaultValue;
    }

    internal ParameterAttributes Attributes { get; }

    public string Name { get; }

    public FunctionOrAttributeHandle ParentHandle { get; private set; }

    public FunctionOrAttribute Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            UnionInterlocked.Initialize(ref field, _reader.GetFunctionOrAttribute(ParentHandle));
            return field;
        }
        private set
        {
            field = value;
            ParentHandle = value.Handle;
        }
    }

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            UnionInterlocked.Initialize(ref field, _reader.GetTypeDescriptor(_type));
            return field;
        }
    }

    public ParameterDefaultHandle DefaultValueHandle { get; }
    public ParameterDefaultDefinition? DefaultValue
    {
        get
        {
            if (DefaultValueHandle.IsNone)
                return null;

            if (field is not null)
                return null;

            Debug.Assert(_reader is not null);
            var value = DefaultValueHandle switch
            {
                StringHandle or BlobHandle or InlineConstHandle =>
                    new ConstantParameterDefaultDefinition(
                        _reader.GetConstant((ConstantHandle)DefaultValueHandle)
                    ),
            };

            Interlocked.CompareExchange(ref field, value, null);
            return field;
        }
    }

    public ImmutableArray<CustomAttributeDefinition> CustomAttributes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_reader is not null);
            var mappings = _reader.CustomAttributesByMember[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(_reader.GetCustomAttribute)]
            );
            return field;
        }
        private set;
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(MemberContainerDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeMapper))]
    [UseStaticMapper(typeof(FunctionDefinition.Mapper))]
    [UseStaticMapper(typeof(CustomAttributeDefinition.Mapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(ParentHandle))]
        public static partial ParameterDefinition ToDefinition(
            ParameterDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );

        private static FunctionOrAttribute ToDefinition(
            FunctionOrAttributeBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        )
        {
            return builder switch
            {
                FunctionDefinitionBuilder b => b.ToDefinition(handler),
                TypeDefinitionBuilder b => b.ToDefinition(handler),
            };
        }
    }
}
