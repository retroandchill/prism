// @file VariableDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

public sealed partial class VariableDefinition : IAttributeHolderDefinition
{
    public string Name { get; }
    private PublicationOnlyValue<ConstantValue?> _constantValue;
    public VariableHandle Handle { get; }
    private readonly TypeHandle _type;
    private readonly ConstantHandle _constantHandle;
    private readonly MetadataReader? _reader;

    internal VariableDefinition(
        VariableHandle handle,
        VariableAttributes attributes,
        string name,
        MemberContainerDefHandle parent,
        TypeHandle type,
        ConstantHandle constantValue,
        MetadataReader reader
    )
    {
        Handle = handle;
        _type = type;
        _constantHandle = constantValue;
        _reader = reader;
        Name = name;
        Attributes = attributes;
        ParentHandle = parent;
    }

    [MapperConstructor]
    internal VariableDefinition(
        VariableHandle handle,
        VariableAttributes attributes,
        string name,
        TypeDescriptor type,
        ConstantValue? constantValue
    )
    {
        Handle = handle;
        Name = name;
        Attributes = attributes;
        Type = type;
        _constantValue = PublicationOnlyValue.Create(constantValue);
    }

    internal VariableAttributes Attributes { get; }

    public VariableKind Kind => Attributes.Kind;

    public DeclaredVisibility Visibility => Attributes.Visibility;

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

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object?>(ref field),
                _reader.GetTypeDescriptor(_type).Value,
                null
            );
            return field;
        }
    }

    public ConstantValue? ConstantValue
    {
        get { return _constantValue.GetOrInitialize(this, static @this =>
            {
                Debug.Assert(@this._reader is not null);
                return !@this._constantHandle.IsNone
                    ? @this._reader.GetConstant(@this._constantHandle)
                    : (ConstantValue?)null;
            }); }
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
    [UseStaticMapper(typeof(CustomAttributeDefinition.Mapper))]
    internal static partial class Mapper
    {
        [MapperIgnoreTarget(nameof(_constantValue))]
        public static partial VariableDefinition ToDefinition(
            VariableDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
