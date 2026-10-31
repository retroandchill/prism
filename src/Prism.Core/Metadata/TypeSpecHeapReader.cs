// @file TypeSpecHeapReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal sealed class TypeSpecHeapReader(ReadOnlyMemory<byte> buffer, int totalItems)
    : HeapReader<TypeSpecRow>(buffer, totalItems, TypeSpecMetadata.HandleShift),
        IHeapReader<TypeSpecHeapReader>
{
    protected override TypeSpecRow Read(ref ArchiveReader reader)
    {
        return reader.ReadValue<TypeSpecRow>().RequireNonNull();
    }

    static TypeSpecHeapReader IHeapReader<TypeSpecHeapReader>.Create(
        ReadOnlyMemory<byte> buffer,
        int totalItems
    )
    {
        return new TypeSpecHeapReader(buffer, totalItems);
    }
}
