// @file ParameterDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal closed record ParameterDefaultDefinition;

internal sealed record ConstantParameterDefaultDefinition(ConstantValue Value)
    : ParameterDefaultDefinition;

internal sealed class ParameterDefinition(
    ParameterHandle handle,
    ParameterAttributes attributes,
    string name,
    TypeHandle type,
    ParameterDefaultHandle defaultValue,
    MetadataReader reader
)
{
    public string Name { get; } = name;

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, reader.GetTypeDescriptor(type));
            return field;
        }
    }

    public ParameterDefaultDefinition? DefaultValue
    {
        get
        {
            if (defaultValue.IsNone)
                return null;

            if (field is not null)
                return null;

            var value = defaultValue switch
            {
                StringHandle or BlobHandle or InlineConstHandle =>
                    new ConstantParameterDefaultDefinition(
                        reader.GetConstant((ConstantHandle)defaultValue)
                    ),
            };

            Interlocked.CompareExchange(ref field, value, null);
            return field;
        }
    }

    public ImmutableArray<CustomAttributeDefinition> Attributes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = reader.CustomAttributesByMember[handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(reader.GetCustomAttribute)]
            );
            return field;
        }
    }
}
