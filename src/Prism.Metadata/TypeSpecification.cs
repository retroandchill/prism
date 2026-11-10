// @file TypeSpecification.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Riok.Mapperly.Abstractions;

namespace Prism.Metadata;

public union TypeDescriptor(TypeDefinition, TypeReference, TypeSpecification);

public closed class TypeSpecification;

public sealed class ArrayTypeSpecification : TypeSpecification
{
    private readonly TypeHandle _elementType;
    private readonly MetadataReader? _reader;

    internal ArrayTypeSpecification(TypeHandle elementType, ulong? size, MetadataReader reader)
    {
        _elementType = elementType;
        _reader = reader;
        Size = size;
    }

    [MapperConstructor]
    internal ArrayTypeSpecification(TypeDescriptor elementType, ulong? size)
    {
        ElementType = elementType;
        Size = size;
    }

    public TypeDescriptor ElementType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object>(ref field),
                _reader.GetTypeDescriptor(_elementType),
                null
            );
            return field;
        }
        private init;
    }

    public ulong? Size { get; }
}

public sealed class ReferenceTypeSpecification : TypeSpecification
{
    private readonly TypeHandle _elementType;
    private readonly MetadataReader? _reader;

    internal ReferenceTypeSpecification(
        TypeHandle elementType,
        bool isMutable,
        MetadataReader reader
    )
    {
        _elementType = elementType;
        _reader = reader;
        IsMutable = isMutable;
    }

    [MapperConstructor]
    internal ReferenceTypeSpecification(TypeDescriptor elementType, bool isMutable)
    {
        ElementType = elementType;
        IsMutable = isMutable;
    }

    public TypeDescriptor ElementType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object>(ref field),
                _reader.GetTypeDescriptor(_elementType),
                null
            );
            return field;
        }
        private init;
    }

    public bool IsMutable { get; }
}

public sealed class NullableTypeSpecification : TypeSpecification
{
    private readonly TypeHandle _elementType;
    private readonly MetadataReader? _reader;

    internal NullableTypeSpecification(TypeHandle elementType, MetadataReader reader)
    {
        _elementType = elementType;
        _reader = reader;
    }

    [MapperConstructor]
    internal NullableTypeSpecification(TypeDescriptor elementType)
    {
        ElementType = elementType;
    }

    public TypeDescriptor ElementType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Debug.Assert(_reader is not null);
            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object>(ref field),
                _reader.GetTypeDescriptor(_elementType),
                null
            );
            return field;
        }
        private init;
    }
}
