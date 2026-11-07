// @file MetadataDescriptor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagicArchive;
using MagicArchive.Utilities;

namespace Prism.Core.Metadata;

internal readonly struct MetadataHeap(ReadOnlyMemory<byte> data, int count)
{
    public ReadOnlySpan<byte> Span => Memory.Span;

    public ReadOnlyMemory<byte> Memory { get; } = data;

    public int Count { get; } = count;
}

[Archivable(GenerateType.Custom)]
internal partial struct MetadataLayout
{
    public MetadataHeader Header { get; private set; }

    public MetadataHeap StringHeap { get; private set; }

    public MetadataHeap BlobHeap { get; private set; }

    public ImmutableArray<AssemblyReferenceRow> AssemblyReferences { get; private set; }

    public ImmutableArray<NamespaceDefRow> NamespaceDefinitions { get; private set; }

    public ImmutableArray<NamespaceRefRow> NamespaceReferences { get; private set; }

    public ImmutableArray<TypeDefinitionRow> TypeDefinitions { get; private set; }

    public ImmutableArray<TypeReferenceRow> TypeReferences { get; private set; }

    public MetadataHeap TypeSpecHeap { get; private set; }

    public ImmutableArray<VariableRow> Variables { get; private set; }

    public ImmutableArray<FunctionRow> Functions { get; private set; }

    public ImmutableArray<ParameterRow> Parameters { get; private set; }

    public ImmutableArray<CustomAttributeRow> CustomAttributes { get; private set; }

    public static MetadataLayout Create(AssemblyDefinition assembly)
    {
        using var strings = new StringHeapBuilder();
        using var blobs = new BlobHeapBuilder();
        using var typeSpecs = new TypeSpecHeapBuilder();

        var mapper = new MetadataRowMapper(strings, blobs, typeSpecs);

        var target = new MetadataLayout
        {
            Header = new MetadataHeader(mapper.Map(assembly.Identity), assembly.TargetInfo),
            AssemblyReferences = mapper.Map(assembly.ReferencedAssemblies),
            NamespaceDefinitions = mapper.Map(assembly.DefinedNamespaces),
            NamespaceReferences = mapper.Map(assembly.ReferencedNamespaces),
            TypeDefinitions = mapper.Map(assembly.DefinedTypes),
            TypeReferences = mapper.Map(assembly.ReferencedTypes),
            Variables = mapper.Map(assembly.Variables),
            Functions = mapper.Map(assembly.Functions),
            Parameters = mapper.Map(assembly.Parameters),
            CustomAttributes = mapper.Map(assembly.CustomAttributes),

            // We have to do the heaps last to ensure everything in them gets written correctly
            StringHeap = FinalizeHeap(strings),
            BlobHeap = FinalizeHeap(blobs),
            TypeSpecHeap = FinalizeHeap(typeSpecs),
        };

        return target;
    }

    private static MetadataHeap FinalizeHeap(IHeapBuilder builder)
    {
        return new MetadataHeap(builder.Data, builder.Count);
    }

    static void IArchivable<MetadataLayout>.Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in MetadataLayout value
    )
    {
        writer.WriteBlittable(value.Header);

        Span<SectionHeader> sectionHeaders = stackalloc SectionHeader[Sections.Length];
        for (var i = 0; i < Sections.Length; i++)
        {
            sectionHeaders[i] = new SectionHeader(0, 0, 0, 0);
        }

        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            var currentSection = 0;

            var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);

            WriteHeap(
                ref tempWriter,
                value.StringHeap,
                sectionHeaders,
                Section.StringTable,
                ref currentSection
            );
            WriteHeap(
                ref tempWriter,
                value.BlobHeap,
                sectionHeaders,
                Section.Blobs,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.AssemblyReferences,
                sectionHeaders,
                Section.AssemblyReferences,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.NamespaceDefinitions,
                sectionHeaders,
                Section.NamespaceDefs,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.NamespaceReferences,
                sectionHeaders,
                Section.NamespaceRefs,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.TypeDefinitions,
                sectionHeaders,
                Section.TypeDefs,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.TypeReferences,
                sectionHeaders,
                Section.TypeRefs,
                ref currentSection
            );
            WriteHeap(
                ref writer,
                value.TypeSpecHeap,
                sectionHeaders,
                Section.TypeSpecs,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.Variables,
                sectionHeaders,
                Section.Variables,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.Functions,
                sectionHeaders,
                Section.Functions,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.Parameters,
                sectionHeaders,
                Section.Parameters,
                ref currentSection
            );
            WriteRows(
                ref tempWriter,
                value.CustomAttributes,
                sectionHeaders,
                Section.CustomAttributes,
                ref currentSection
            );

            tempWriter.Flush();

            writer.WriteSpan(sectionHeaders);
            tempBuffer.WriteToAndReset(ref writer);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
        }
    }

    private static void WriteHeap<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        MetadataHeap heap,
        scoped Span<SectionHeader> sectionHeaders,
        Section section,
        ref int sectionIndex
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        sectionHeaders[sectionIndex++] = new SectionHeader(
            section,
            writer.WrittenBytes,
            heap.Memory.Length,
            heap.Count
        );
        writer.WriteMetadataBytes(heap.Span);
        writer.Flush();
    }

    private static void WriteRows<T, TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        ImmutableArray<T> rows,
        scoped Span<SectionHeader> sectionHeaders,
        Section section,
        ref int sectionIndex
    )
        where T : struct, IArchivable<T>, IFixedSizeArchivable
        where TBufferWriter : IBufferWriter<byte>
    {
        sectionHeaders[sectionIndex++] = new SectionHeader(
            section,
            writer.WrittenBytes,
            T.Size * rows.Length,
            rows.Length
        );
        writer.WriteMetadataRows(rows.AsSpan());
        writer.Flush();
    }

    static void IArchivable<MetadataLayout>.Deserialize(
        ref ArchiveReader reader,
        scoped ref MetadataLayout value
    )
    {
        value.Header = reader.ReadBlittable<MetadataHeader>();

        SectionHeader[]? rentedHeaders = null;
        try
        {
            Span<SectionHeader> sectionHeaders = stackalloc SectionHeader[Sections.Length];
            if (!reader.TryReadInto(sectionHeaders, out var length))
            {
                rentedHeaders = ArrayPool<SectionHeader>.Shared.Rent(length);
                sectionHeaders = rentedHeaders.AsSpan(0, length);
            }

            var read = reader.TryReadInto(sectionHeaders, out _);
            Debug.Assert(read);

            sectionHeaders.Sort((a, b) => a.Offset.CompareTo(b.Offset));

            var start = reader.ConsumedBytes;
            foreach (var header in sectionHeaders)
            {
                Debug.Assert(header.Offset == reader.ConsumedBytes - start);
                ReadSection(ref reader, header, ref value);
            }
        }
        finally
        {
            if (rentedHeaders is not null)
                ArrayPool<SectionHeader>.Shared.Return(rentedHeaders);
        }
    }

    private static void ReadSection(
        ref ArchiveReader reader,
        SectionHeader header,
        scoped ref MetadataLayout value
    )
    {
        switch (header.Kind)
        {
            case Section.StringTable:
                value.StringHeap = ReadMetadataHeap(ref reader, header.Size, header.Count);
                break;
            case Section.Blobs:
                value.BlobHeap = ReadMetadataHeap(ref reader, header.Size, header.Count);
                break;
            case Section.AssemblyReferences:
                value.AssemblyReferences = ReadMetadataRows<AssemblyReferenceRow>(
                    ref reader,
                    header.Count
                );
                break;
            case Section.NamespaceDefs:
                value.NamespaceDefinitions = ReadMetadataRows<NamespaceDefRow>(
                    ref reader,
                    header.Count
                );
                break;
            case Section.NamespaceRefs:
                value.NamespaceReferences = ReadMetadataRows<NamespaceRefRow>(
                    ref reader,
                    header.Count
                );
                break;
            case Section.TypeDefs:
                value.TypeDefinitions = ReadMetadataRows<TypeDefinitionRow>(
                    ref reader,
                    header.Count
                );
                break;
            case Section.TypeRefs:
                value.TypeReferences = ReadMetadataRows<TypeReferenceRow>(ref reader, header.Count);
                break;
            case Section.TypeSpecs:
                value.TypeSpecHeap = ReadMetadataHeap(ref reader, header.Size, header.Count);
                break;
            case Section.Variables:
                value.Variables = ReadMetadataRows<VariableRow>(ref reader, header.Count);
                break;
            case Section.Functions:
                value.Functions = ReadMetadataRows<FunctionRow>(ref reader, header.Count);
                break;
            case Section.Parameters:
                value.Parameters = ReadMetadataRows<ParameterRow>(ref reader, header.Count);
                break;
            case Section.CustomAttributes:
                value.CustomAttributes = ReadMetadataRows<CustomAttributeRow>(
                    ref reader,
                    header.Count
                );
                break;
            default:
                throw new ArchiveSerializationException($"Unexpected section kind: {header.Kind}");
        }
    }

    private static MetadataHeap ReadMetadataHeap(ref ArchiveReader reader, int length, int count)
    {
        ref var start = ref reader.GetSpanReference(length);
        var span = MemoryMarshal.CreateReadOnlySpan(ref start, length);
        var result = new MetadataHeap(span.ToArray(), count);
        reader.Advance(length);
        return result;
    }

    private static ImmutableArray<T> ReadMetadataRows<T>(ref ArchiveReader reader, int count)
        where T : IFixedSizeArchivable
    {
        var rows = reader.ReadMetadataRows<T>(count);
        return ImmutableCollectionsMarshal.AsImmutableArray(rows);
    }
}
