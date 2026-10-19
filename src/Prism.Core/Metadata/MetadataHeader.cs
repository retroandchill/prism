// @file MetadataHeader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct MetadataHeader(
    AssemblyIdentity AssemblyIdentity,
    TargetInfo TargetInfo
)
{
    private static readonly ImmutableArray<byte> HeaderSequence = [.. "PRISM"u8];
    private const ushort Version = 1;

    [ArchivableOnSerializing]
    private static void WriteHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        [UsedImplicitly] in MetadataHeader header
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        foreach (var b in HeaderSequence)
        {
            writer.WriteBlittable(b);
        }
        writer.WriteBlittable(Version);
    }

    [ArchivableOnDeserializing]
    private static void ReadHeader(
        ref ArchiveReader reader,
        [UsedImplicitly] ref MetadataHeader value
    )
    {
        foreach (var b in HeaderSequence)
        {
            if (reader.ReadBlittable<byte>() != b)
            {
                throw new SerializationException("Invalid header sequence");
            }
        }

        var version = reader.ReadBlittable<ushort>();
        if (version != Version)
        {
            throw new SerializationException("Invalid version");
        }
    }
}
