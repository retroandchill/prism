// @file TypeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Core.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Core.Metadata;

public union NamedType(TypeDefinition, TypeReference)
{
    public NamedTypeHandle Handle
    {
        get
        {
            return this switch
            {
                TypeDefinition typeDefinition => typeDefinition.Handle,
                TypeReference typeReference => typeReference.Handle,
            };
        }
    }
}

public sealed class TypeDefinition : MemberContainerDefinition
{
    internal TypeDefinition(
        TypeDefHandle handle,
        NamedTypeAttributes attributes,
        string name,
        MemberContainerDefHandle parent,
        NamedTypeHandle baseType,
        MetadataReader reader
    )
        : base(name, reader)
    {
        Handle = handle;
        Attributes = attributes;
        ParentHandle = parent;
        BaseTypeHandle = baseType;
    }

    [MapperConstructor]
    internal TypeDefinition(TypeDefHandle handle, NamedTypeAttributes attributes, string name)
        : base(name)
    {
        Handle = handle;
        Attributes = attributes;
    }

    public new TypeDefHandle Handle { get; }

    internal NamedTypeAttributes Attributes { get; }

    public NamedTypeKind Kind => Attributes.Kind;

    public DeclaredVisibility Visibility => Attributes.Visibility;

    public MemberContainerDefHandle ParentHandle { get; private set; }

    public MemberContainerDefinition? Parent
    {
        get
        {
            if (ParentHandle.IsNone)
                return null;

            if (field is not null)
                return field;

            Debug.Assert(MetadataReader is not null);
            Interlocked.CompareExchange(
                ref field,
                MetadataReader.GetMemberContainerDef(ParentHandle),
                null
            );
            return field;
        }
        internal set;
    }

    public NamedTypeHandle BaseTypeHandle { get; private set; }

    private NamedType _lazyBaseType;

    public NamedType? BaseType
    {
        get
        {
            if (BaseTypeHandle.IsNone)
                return null;

            if (_lazyBaseType.Value is not null)
                return _lazyBaseType;

            Interlocked.CompareExchange(
                ref Unsafe.As<NamedType, object?>(ref _lazyBaseType),
                MetadataReader.GetNamedType(BaseTypeHandle).Value,
                null
            );
            return _lazyBaseType;
        }
        internal set
        {
            if (value is { } namedType)
            {
                _lazyBaseType = namedType;
                BaseTypeHandle = namedType.Handle;
            }
            else
            {
                _lazyBaseType = default;
                BaseTypeHandle = NamedTypeHandle.None;
            }
        }
    }

    public ImmutableArray<ParameterDefinition> AttributeParameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = MetadataReader.ParametersByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetParameter)]
            );
            return field;
        }
        internal set;
    }

    public ImmutableArray<CustomAttributeDefinition> CustomAttributes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = MetadataReader.CustomAttributesByMember[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetCustomAttribute)]
            );
            return field;
        }
        internal set;
    }

    protected override MemberContainerDefHandle GetHandle() => Handle;
}
