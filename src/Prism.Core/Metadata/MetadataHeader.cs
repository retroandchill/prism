// @file MetadataHeader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MagicArchive;
using NetEscapades.EnumGenerators;
using Prism.Core.Configuration;

namespace Prism.Core.Metadata;

[Archivable]
internal readonly partial record struct MetadataHeader(
    AssemblyIdentity AssemblyIdentity,
    TargetInfo TargetInfo
)
{
    public const uint HeaderSequence = 0x50524953; // PRIS

    public const ushort Version = 1;

    [ArchivableOnSerializing]
    private static void WriteHeader<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        in MetadataHeader header
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBlittable(HeaderSequence);
        writer.WriteBlittable(Version);
    }

    [ArchivableOnDeserializing]
    private static void ReadHeader(ref ArchiveReader reader, ref MetadataHeader value)
    {
        var headerSequence = reader.ReadBlittable<uint>();
        if (headerSequence != HeaderSequence)
        {
            throw new SerializationException("Invalid header sequence");
        }

        var version = reader.ReadBlittable<ushort>();
        if (version != Version)
        {
            throw new SerializationException("Invalid version");
        }
    }
}
