// @file TypeSpecBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Metadata.Symbols;

namespace Prism.Metadata.Builders;

public closed class TypeSpecBuilder;

public sealed class ArrayTypeBuilder : TypeSpecBuilder
{
    public TypeDescriptorBuilder ElementType { get; }

    public ulong? Size { get; }

    internal ArrayTypeBuilder(TypeDescriptorBuilder elementType, ulong? size)
    {
        ElementType = elementType;
        Size = size;
    }
}

public sealed class ReferenceTypeBuilder : TypeSpecBuilder
{
    public TypeDescriptorBuilder ElementType { get; }

    public bool IsMutable { get; }

    internal ReferenceTypeBuilder(TypeDescriptorBuilder elementType, bool isMutable)
    {
        ElementType = elementType;
        IsMutable = isMutable;
    }
}

public sealed class NullableTypeBuilder : TypeSpecBuilder
{
    public TypeDescriptorBuilder ElementType { get; }

    internal NullableTypeBuilder(TypeDescriptorBuilder elementType)
    {
        ElementType = elementType;
    }
}
