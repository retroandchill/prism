// @file ParameterDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

public closed record ParameterDefaultDefinition;

public sealed record ConstantParameterDefaultDefinition(ConstantValue Value)
    : ParameterDefaultDefinition;

public union FunctionOrAttribute(TypeDefinition, FunctionDefinition);

public sealed class ParameterDefinition
{
    private readonly ParameterHandle _handle;
    private readonly TypeHandle _type;
    private readonly MetadataReader _reader;

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
        _handle = handle;
        _type = type;
        _reader = reader;
        Attributes = attributes;
        Name = name;
        ParentHandle = parent;
        DefaultValueHandle = defaultValue;
    }

    internal ParameterAttributes Attributes { get; }

    public string Name { get; }

    public FunctionOrAttributeHandle ParentHandle { get; }

    public FunctionOrAttribute Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, _reader.GetFunctionOrAttribute(ParentHandle));
            return field;
        }
    }

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

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

            var mappings = _reader.CustomAttributesByMember[_handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(_reader.GetCustomAttribute)]
            );
            return field;
        }
    }
}
