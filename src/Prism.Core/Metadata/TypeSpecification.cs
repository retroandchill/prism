// @file TypeSpecification.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Core.Metadata;

internal union TypeDescriptor(TypeDefinition, TypeReference, TypeSpecification);

internal closed class TypeSpecification;

internal sealed class ArrayTypeSpecification(
    TypeHandle elementType,
    ulong? size,
    MetadataReader reader
) : TypeSpecification
{
    public TypeDescriptor ElementType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object>(ref field),
                reader.GetTypeDescriptor(elementType),
                null
            );
            return field;
        }
    }

    public ulong? Size { get; } = size;
}

internal sealed class ReferenceTypeSpecification(
    TypeHandle elementType,
    bool isMutable,
    MetadataReader reader
) : TypeSpecification
{
    public TypeDescriptor ElementType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object>(ref field),
                reader.GetTypeDescriptor(elementType),
                null
            );
            return field;
        }
    }

    public bool IsMutable { get; } = isMutable;
}

internal sealed class NullableTypeSpecification(TypeHandle elementType, MetadataReader reader)
    : TypeSpecification
{
    public TypeDescriptor ElementType
    {
        get
        {
            if (field.Value is not null)
                return field;

            Interlocked.CompareExchange(
                ref Unsafe.As<TypeDescriptor, object>(ref field),
                reader.GetTypeDescriptor(elementType),
                null
            );
            return field;
        }
    }
}
