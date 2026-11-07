// @file CustomAttributeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

public union AttributeHolderDefinition(
    TypeDefinition,
    VariableDefinition,
    FunctionDefinition,
    ParameterDefinition
);

public sealed class CustomAttributeDefinition
{
    private readonly BlobHandle _arguments;
    private readonly MetadataReader _reader;

    internal CustomAttributeDefinition(
        AttributeHolderHandle parent,
        TypeHandle type,
        BlobHandle arguments,
        MetadataReader reader
    )
    {
        _arguments = arguments;
        _reader = reader;
        ParentHandle = parent;
        TypeHandle = type;
    }

    public AttributeHolderHandle ParentHandle { get; }

    public AttributeHolderDefinition Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, _reader.GetAttributeHolder(ParentHandle));
            return field;
        }
    }

    public TypeHandle TypeHandle { get; }

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, _reader.GetTypeDescriptor(TypeHandle));
            return field;
        }
    }

    public ImmutableArray<ConstantValue> Arguments
    {
        get
        {
            if (!field.IsDefault)
                return field;

            if (_arguments.IsNone)
            {
                ImmutableInterlocked.InterlockedInitialize(ref field, []);
                return field;
            }

            var argHandles = _reader.GetBlob<ImmutableArray<ConstantHandle>>(_arguments);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. argHandles.Select(_reader.GetConstant)]
            );
            return field;
        }
    }
}
