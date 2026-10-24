// @file MetadataBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using MagicArchive;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

internal sealed class MetadataBuilder : IDisposable
{
    private readonly MetadataHeader _header;

    private readonly StringHeapBuilder _strings = new();
    private readonly BlobHeapBuilder _blobs = new();

    private readonly Dictionary<AssemblyIdentity, MetadataToken> _seenAssemblyReferences = new();
    private readonly List<AssemblyIdentityData> _referencedAssemblies = [];

    private readonly List<NamespaceRow> _namespaces = [];

    public MetadataBuilder(AssemblyIdentity identity, TargetInfo target)
    {
        _header = new MetadataHeader(ConvertAssemblyIdentity(identity), target);
        _seenAssemblyReferences.Add(identity, new MetadataToken(TokenType.Assembly, 1));
    }

    public StringHandle GetOrAddString(string value)
    {
        return _strings.GetOrAdd(value);
    }

    public void AddReferencedAssembly(AssemblyIdentity identity)
    {
        if (_seenAssemblyReferences.ContainsKey(identity))
            return;

        var data = ConvertAssemblyIdentity(identity);
        var id = AssemblyRefHandle.FromRowId(_referencedAssemblies.Count + 1);
        _seenAssemblyReferences.Add(identity, id);
        _referencedAssemblies.Add(data);
    }

    public NamespaceHandle AddNamespace(NamespaceRow row)
    {
        _namespaces.Add(row);
        return NamespaceHandle.FromRowId(_namespaces.Count);
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

        var currentSection = 0;

        WriteHeap(
            ref writer,
            ref bufferWriter,
            _strings,
            sectionHeaders,
            Section.StringTable,
            ref currentSection
        );
        WriteHeap(
            ref writer,
            ref bufferWriter,
            _blobs,
            sectionHeaders,
            Section.BinaryBlobs,
            ref currentSection
        );
        WriteRows(
            ref writer,
            ref bufferWriter,
            _referencedAssemblies,
            sectionHeaders,
            Section.AssemblyReferences,
            ref currentSection
        );
        WriteRows(
            ref writer,
            ref bufferWriter,
            _namespaces,
            sectionHeaders,
            Section.Namespaces,
            ref currentSection
        );

        var targetMemory = bufferWriter.WrittenMemory[sectionStart..sectionEnd];
        var backPatchWriter = new BackPatchWriter(MemoryMarshal.AsMemory(targetMemory));
        var subWriter = ArchiveWriter.Create(ref backPatchWriter, state);
        subWriter.WriteSpan(sectionHeaders);
        subWriter.Flush();
    }

    private void WriteHeap(
        ref ArchiveWriter<MetadataBuffer> writer,
        ref MetadataBuffer bufferWriter,
        IHeapBuilder heap,
        scoped Span<SectionHeader> sectionHeaders,
        Section section,
        ref int sectionIndex
    )
    {
        sectionHeaders[sectionIndex++] = new SectionHeader(
            section,
            bufferWriter.Position,
            _strings.Data.Length,
            _strings.Count
        );
        writer.WriteMetadataBytes(heap.Data);
        writer.Flush();
    }

    private void WriteRows<T>(
        ref ArchiveWriter<MetadataBuffer> writer,
        ref MetadataBuffer bufferWriter,
        List<T> rows,
        scoped Span<SectionHeader> sectionHeaders,
        Section section,
        ref int sectionIndex
    )
        where T : struct, IArchivable<T>, IFixedSizeArchivable
    {
        sectionHeaders[sectionIndex++] = new SectionHeader(
            section,
            bufferWriter.Position,
            T.Size * rows.Count,
            rows.Count
        );
        writer.WriteMetadataRows(CollectionsMarshal.AsSpan(rows));
        writer.Flush();
    }

    public void Dispose()
    {
        _strings.Dispose();
    }
}
