// @file MetadataHeader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MagicArchive;

namespace Prism.Core.Metadata;

[Archivable(GenerateType.Custom)]
internal readonly partial record struct MetadataHeader(
    AssemblyIdentityData AssemblyIdentity,
    TargetInfo TargetInfo
)
{
    private static readonly ImmutableArray<byte> HeaderSequence = [.. "PRISM"u8];
    private const ushort Version = 1;

    static void IArchivable<MetadataHeader>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in MetadataHeader value
    )
    {
        WriteHeader(ref writer);
        writer.WriteBlittable(Version, value.AssemblyIdentity, value.TargetInfo);
    }

    private static void WriteHeader<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        ref var spanReference = ref writer.GetSpanReference(HeaderSequence.Length);
        ref var src = ref MemoryMarshal.GetReference(HeaderSequence.AsSpan());
        Unsafe.CopyBlock(ref spanReference, ref src, (uint)HeaderSequence.Length);
        writer.Advance(HeaderSequence.Length);
    }

    static void IArchivable<MetadataHeader>.Deserialize(
        ref ArchiveReader reader,
        scoped ref MetadataHeader value
    )
    {
        ReadHeader(ref reader);

        var version = reader.ReadBlittable<ushort>();
        if (version != Version)
        {
            throw new ArchiveSerializationException("Invalid version");
        }

        reader.ReadBlittable(out AssemblyIdentityData assemblyIdentity, out TargetInfo targetInfo);
        value = new MetadataHeader(assemblyIdentity, targetInfo);
    }

    private static void ReadHeader(ref ArchiveReader reader)
    {
        ref var src = ref reader.GetSpanReference(HeaderSequence.Length);

        Span<byte> destSpan = stackalloc byte[HeaderSequence.Length];
        ref var dest = ref MemoryMarshal.GetReference(destSpan);
        Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)HeaderSequence.Length);
        reader.Advance(HeaderSequence.Length);

        if (!destSpan.SequenceEqual(HeaderSequence.AsSpan()))
        {
            throw new ArchiveSerializationException("Invalid header");
        }
    }
}
