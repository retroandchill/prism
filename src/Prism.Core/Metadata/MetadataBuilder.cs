// @file MetadataBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class MetadataBuilder : IDisposable
{
    private readonly MetadataHeader _header;

    private readonly StringHeapBuilder _strings = new();
    private readonly BlobHeapBuilder _blobs = new();

    public MetadataBuilder(AssemblyIdentity identity, TargetInfo target)
    {
        _header = new MetadataHeader(ConvertAssemblyIdentity(identity), target);
    }

    private AssemblyIdentityData ConvertAssemblyIdentity(AssemblyIdentity identity)
    {
        var nameHandle = _strings.GetOrAdd(identity.Name);
        return new AssemblyIdentityData(nameHandle, identity.Version);
    }

    public async ValueTask WriteAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new MetadataBuffer();
        Write(ref buffer);
        await buffer.CopyToAsync(stream, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private void Write(ref MetadataBuffer bufferWriter)
    {
        using var state = ArchiveWriterStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var writer = ArchiveWriter.Create(ref bufferWriter, state);
        writer.WriteArchivable(_header);
        writer.Flush();
        WriteSections(ref bufferWriter, state);
    }

    private void WriteSections(ref MetadataBuffer bufferWriter, ArchiveWriterState state)
    {
        var sectionStart = bufferWriter.Position;
        Span<SectionHeader> sectionHeaders = stackalloc SectionHeader[Sections.Length];
        for (var i = 0; i < Sections.Length; i++)
        {
            sectionHeaders[i] = new SectionHeader(0, bufferWriter.Position, 0, 0);
        }

        var writer = ArchiveWriter.Create(ref bufferWriter, state);
        writer.WriteSpan(sectionHeaders);
        writer.Flush();
        var sectionEnd = bufferWriter.Position;

        sectionHeaders[0] = new SectionHeader(
            Section.StringTable,
            bufferWriter.Position,
            _strings.Data.Length,
            _strings.Count
        );
        writer.WriteMetadataBytes(_strings.Data);
        writer.Flush();

        sectionHeaders[1] = new SectionHeader(
            Section.BinaryBlobs,
            bufferWriter.Position,
            _blobs.Data.Length,
            _blobs.Count
        );
        writer.WriteMetadataBytes(_blobs.Data);
        writer.Flush();

        sectionHeaders[2] = new SectionHeader(
            Section.AssemblyReferences,
            bufferWriter.Position,
            0,
            0
        );
        writer.WriteMetadataBytes([]);
        writer.Flush();

        var targetMemory = bufferWriter.WrittenMemory[sectionStart..sectionEnd];
        var backPatchWriter = new BackPatchWriter(MemoryMarshal.AsMemory(targetMemory));
        var subWriter = ArchiveWriter.Create(ref backPatchWriter, state);
        subWriter.WriteSpan(sectionHeaders);
        subWriter.Flush();
    }

    public void Dispose()
    {
        _strings.Dispose();
    }
}
