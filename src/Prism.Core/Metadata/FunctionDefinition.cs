// @file FunctionDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Abi;
using Prism.Core.Symbols;
using Prism.Core.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

public sealed partial class FunctionDefinition : IAttributeHolderDefinition
{
    private TypeDescriptor _lazyReceiverType;
    public FunctionHandle Handle { get; }
    private readonly MetadataReader? _reader;

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
        Handle = handle;
        _reader = reader;
        Attributes = attributes;
        Name = name;
        ParentHandle = parent;
        ReturnTypeHandle = returnType;
        ReceiverTypeHandle = receiverType;
    }

    [MapperConstructor]
    private FunctionDefinition(
        FunctionHandle handle,
        FunctionAttributes attributes,
        string name,
        TypeDescriptor returnType,
        TypeDescriptor? receiverType
    )
    {
        Handle = handle;
        Attributes = attributes;
        Name = name;
        ReturnType = returnType;
        ReceiverType = receiverType;
    }

    internal FunctionAttributes Attributes { get; }

    public DeclaredVisibility Visibility => Attributes.Visibility;

    public bool IsExtern => Attributes.IsExtern;

    public AbiKind AbiKind => Attributes.AbiKind;

    public FunctionKind Kind => Attributes.Kind;

    public string Name { get; }

    public MemberContainerDefHandle ParentHandle { get; }

    public MemberContainerDefinition Parent
    {
        get
        {
            if (field is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(
                ref field,
                _reader.GetMemberContainerDef(ParentHandle),
                null
            );
            return field;
        }
        private set;
    }

    public TypeHandle ReturnTypeHandle { get; }
    public TypeDescriptor ReturnType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            UnionInterlocked.Initialize(ref field, _reader.GetTypeDescriptor(ReturnTypeHandle));
            return field;
        }
    }

    public TypeHandle ReceiverTypeHandle { get; private init; }

    public TypeDescriptor? ReceiverType
    {
        get
        {
            if (ReceiverTypeHandle.IsNone)
                return null;

            if (_lazyReceiverType.Value is not null)
                return _lazyReceiverType;

            Debug.Assert(_reader is not null);
            UnionInterlocked.Initialize(
                ref _lazyReceiverType,
                _reader.GetTypeDescriptor(ReceiverTypeHandle)
            );
            return _lazyReceiverType;
        }
        private init
        {
            if (value is { } type)
            {
                _lazyReceiverType = type;
            }
            else
            {
                ReceiverTypeHandle = TypeHandle.None;
            }
        }
    }

    public ImmutableArray<ParameterDefinition> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_reader is not null);
            var mappings = _reader.ParametersByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(_reader.GetParameter)]
            );
            return field;
        }
        private set;
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
    [UseStaticMapper(typeof(ParameterDefinition.Mapper))]
    [UseStaticMapper(typeof(CustomAttributeDefinition.Mapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(ReceiverTypeHandle))]
        [MapperIgnoreTarget(nameof(_lazyReceiverType))]
        public static partial FunctionDefinition ToDefinition(
            FunctionDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
