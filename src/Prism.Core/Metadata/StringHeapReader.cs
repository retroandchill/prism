// @file StringHeapReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class StringHeapReader(ReadOnlyMemory<byte> buffer, int count)
    : HeapReader<string>(buffer, count, KnownStrings.HandleShift),
        IHeapReader<StringHeapReader>
{
    protected override string Read(ref ArchiveReader reader)
    {
        return reader.ReadMetadataString();
    }

    protected override string ReadSpecialCase(int id)
    {
        var knownString = (KnownString)id;
        return knownString.ToStringFast(true);
    }

    static StringHeapReader IHeapReader<StringHeapReader>.Create(
        ReadOnlyMemory<byte> buffer,
        int totalItems
    )
    {
        return new StringHeapReader(buffer, totalItems);
    }
}
