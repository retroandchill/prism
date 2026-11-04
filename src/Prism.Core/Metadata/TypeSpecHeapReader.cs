// @file TypeSpecHeapReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal sealed class TypeSpecHeapReader(
    ReadOnlyMemory<byte> buffer,
    int totalItems,
    MetadataReader metadataReader
)
    : HeapReader<TypeSpecification>(buffer, totalItems, TypeSpecMetadata.HandleShift),
        IHeapReader<TypeSpecHeapReader>
{
    protected override TypeSpecification Read(ref ArchiveReader reader)
    {
        var specRow = reader.ReadValue<TypeSpecRow>().RequireNonNull();
        return specRow switch
        {
            ArrayTypeRow(var element, var size) => new ArrayTypeSpecification(
                element,
                size,
                metadataReader
            ),
            NullableTypeRow(var elementType) => new NullableTypeSpecification(
                elementType,
                metadataReader
            ),
            ReferenceTypeRow(var elementType, var isMutable) => new ReferenceTypeSpecification(
                elementType,
                isMutable,
                metadataReader
            ),
        };
    }

    static TypeSpecHeapReader IHeapReader<TypeSpecHeapReader>.Create(
        ReadOnlyMemory<byte> buffer,
        int totalItems,
        MetadataReader reader
    )
    {
        return new TypeSpecHeapReader(buffer, totalItems, reader);
    }
}
