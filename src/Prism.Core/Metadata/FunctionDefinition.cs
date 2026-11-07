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

public sealed class FunctionDefinition
{
    private TypeDescriptor _lazyReceiverType;
    private readonly FunctionHandle _handle;
    private readonly MetadataReader _reader;

    internal FunctionDefinition(
        FunctionHandle handle,
        FunctionAttributes attributes,
        string name,
        MemberContainerDefHandle parent,
        TypeHandle returnType,
        TypeHandle receiverType,
        MetadataReader reader
    )
    {
        _handle = handle;
        _reader = reader;
        Attributes = attributes;
        Name = name;
        ParentHandle = parent;
        ReturnTypeHandle = returnType;
        ReceiverTypeHandle = receiverType;
    }

    internal FunctionAttributes Attributes { get; }

    public DeclaredVisibility Visibility => Attributes.Visibility;

    public bool IsExtern => Attributes.IsExtern;

    public AbiKind AbiKind => Attributes.AbiKind;

    public string Name { get; }

    public MemberContainerDefHandle ParentHandle { get; }

    public MemberContainerDefinition? Parent
    {
        get
        {
            if (ParentHandle.IsNone)
                return null;

            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _reader.GetMemberContainerDef(ParentHandle),
                null
            );
            return field;
        }
    }

    public TypeHandle ReturnTypeHandle { get; }
    public TypeDescriptor ReturnType
    {
        get
        {
            if (field.Value is not null)
                return field;

            UnionInterlocked.Initialize(ref field, _reader.GetTypeDescriptor(ReturnTypeHandle));
            return field;
        }
    }

    public TypeHandle ReceiverTypeHandle { get; }

    public TypeDescriptor? ReceiverType
    {
        get
        {
            if (ReceiverTypeHandle.IsNone)
                return null;

            if (_lazyReceiverType.Value is not null)
                return _lazyReceiverType;

            UnionInterlocked.Initialize(
                ref _lazyReceiverType,
                _reader.GetTypeDescriptor(ReceiverTypeHandle)
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

            var mappings = _reader.ParametersByParent[_handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(_reader.GetParameter)]
            );
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
