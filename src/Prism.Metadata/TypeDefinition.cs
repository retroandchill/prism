// @file TypeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata;

public sealed partial class TypeDefinition : MemberContainerDefinition, IAttributeHolderDefinition
{
    private readonly TypeHandle _baseType;

    internal TypeDefinition(
        TypeDefHandle handle,
        NamedTypeAttributes attributes,
        string name,
        MemberContainerDefHandle parent,
        TypeHandle baseType,
        MetadataReader reader
    )
        : base(name, reader)
    {
        Handle = handle;
        Attributes = attributes;
        ParentHandle = parent;
        _baseType = baseType;
    }

    [MapperConstructor]
    private TypeDefinition(TypeDefHandle handle, NamedTypeAttributes attributes, string name)
        : base(name)
    {
        Handle = handle;
        Attributes = attributes;
    }

    public new TypeDefHandle Handle { get; }

    internal NamedTypeAttributes Attributes { get; }

    public NamedTypeKind Kind => Attributes.Kind;

    public DeclaredVisibility Visibility => Attributes.Visibility;

    public SpecialType SpecialType => Attributes.SpecialType;

    public MemberContainerDefHandle ParentHandle { get; private set; }

    public MemberContainerDefinition Parent
    {
        get
        {
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
        private set;
    }

    private TypeDescriptor _lazyBaseType;

    public TypeDescriptor? BaseType
    {
        get
        {
            if (_baseType.IsNone)
                return null;

            if (_lazyBaseType.Value is not null)
                return _lazyBaseType;

            Debug.Assert(MetadataReader is not null);
            UnionInterlocked.Initialize(
                ref _lazyBaseType,
                MetadataReader.GetTypeDescriptor(_baseType)
            );
            return _lazyBaseType;
        }
        internal set;
    }

    public ImmutableArray<EnumOrdinalDefinition> EnumOrdinals
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.OrdinalsByParent.GetValueOrDefault(Handle, []);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetEnumOrdinal)]
            );
            return field;
        }
        private set;
    }

    public ImmutableArray<ParameterDefinition> AttributeParameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.ParametersByParent.GetValueOrDefault(Handle, []);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetParameter)]
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

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.CustomAttributesByMember[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetCustomAttribute)]
            );
            return field;
        }
        private set;
    }

    protected override MemberContainerDefHandle GetHandle() => Handle;

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(MemberContainerDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeMapper))]
    [UseStaticMapper(typeof(TypeAlias.Mapper))]
    [UseStaticMapper(typeof(VariableDefinition.Mapper))]
    [UseStaticMapper(typeof(FunctionDefinition.Mapper))]
    [UseStaticMapper(typeof(ParameterDefinition.Mapper))]
    [UseStaticMapper(typeof(EnumOrdinalDefinition.Mapper))]
    [UseStaticMapper(typeof(CustomAttributeDefinition.Mapper))]
    internal static new partial class Mapper
    {
        [MapperIgnoreTarget(nameof(_lazyBaseType))]
        public static partial TypeDefinition ToDefinition(
            TypeDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
