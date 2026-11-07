// @file VariableDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

public sealed class VariableDefinition
{
    public string Name { get; }
    private Lazy<ConstantValue?>? _constantValue;
    private readonly VariableHandle _handle;
    private readonly TypeHandle _type;
    private readonly ConstantHandle _constantValue1;
    private readonly MetadataReader _reader;

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
        _handle = handle;
        _type = type;
        _constantValue1 = constantValue;
        _reader = reader;
        Name = name;
        Attributes = attributes;
        ParentHandle = parent;
    }

    internal VariableAttributes Attributes { get; }

    public VariableKind Kind => Attributes.Kind;

    public DeclaredVisibility Visibility => Attributes.Visibility;

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

    public TypeDescriptor Type
    {
        get
        {
            if (field.Value is not null)
                return field;

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
        get
        {
            if (_constantValue is not null)
                return _constantValue.Value;

            Interlocked.CompareExchange(
                ref _constantValue,
                new Lazy<ConstantValue?>(
                    () =>
                        !_constantValue1.IsNone
                            ? _reader.GetConstant(_constantValue1)
                            : (ConstantValue?)null,
                    LazyThreadSafetyMode.PublicationOnly
                ),
                null
            );
            return _constantValue.Value;
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
