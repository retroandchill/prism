// @file FunctionDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Core.Abi;
using Prism.Core.Symbols;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal sealed class FunctionDefinition(
    FunctionHandle handle,
    FunctionAttributes attributes,
    string name,
    TypeHandle returnType,
    TypeHandle receiverType,
    MetadataReader reader
)
{
    public DeclaredVisibility Visibility => attributes.Visibility;

    public bool IsExtern => attributes.IsExtern;

    public AbiKind AbiKind => attributes.AbiKind;

    public string Name { get; } = name;

    public TypeDescriptor ReturnType
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, reader.GetTypeDescriptor(returnType));
            return field;
        }
    }

    private TypeDescriptor _lazyReceiverType;

    public TypeDescriptor? ReceiverType
    {
        get
        {
            if (receiverType.IsNone)
                return null;

            if (_lazyReceiverType.Value is not null)
                return _lazyReceiverType;

            UnionInterlocked.Initialize(
                ref _lazyReceiverType,
                reader.GetTypeDescriptor(receiverType)
            );
            return _lazyReceiverType;
        }
    }

    public ImmutableArray<ParameterDefinition> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = reader.ParametersByParent[handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(reader.GetParameter)]
            );
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
