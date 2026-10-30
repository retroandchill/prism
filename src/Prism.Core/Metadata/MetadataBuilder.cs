// @file MetadataBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive;
using Prism.Core.Semantic;

namespace Prism.Core.Metadata;

internal sealed class MetadataBuilder : IDisposable
{
    private readonly MetadataHeader _header;

    private readonly StringHeapBuilder _strings = new();
    private readonly SerializedHeapBuilder<ConstDataHandle, ConstantValue> _constantData = new();

    private readonly Dictionary<ConstantValue, ConstantHandle> _constants = new();

    private readonly List<AssemblyIdentityData> _referencedAssemblies = [];

    private readonly List<NamespaceDefRow> _namespaceDefs = [];
    private readonly List<NamespaceRefRow> _namespaceRefs = [];

    private readonly List<TypeDefinitionRow> _typeDefs = [];
    private readonly List<TypeReferenceRow> _typeRefs = [];
    private readonly SerializedHeapBuilder<TypeSpecHandle, TypeSpecRow> _typeSpecHeap = new();

    private readonly List<VariableRow> _variables = [];

    public MetadataBuilder(AssemblyIdentity identity, TargetInfo target)
    {
        _header = new MetadataHeader(ConvertAssemblyIdentity(identity), target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringHandle GetOrAddString(string value)
    {
        return _strings.GetOrAdd(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ConstantHandle GetOrAddConstant(in ConstantValue value)
    {
        if (_constants.TryGetValue(value, out var handle))
        {
            return handle;
        }

        handle = value switch
        {
            string s => GetOrAddString(s),
            _ => StoreNonStringConstant(in value),
        };

        _constants.Add(value, handle);
        return handle;
    }

    private ConstantHandle StoreNonStringConstant(in ConstantValue value)
    {
        if (InlineConstHandle.TryCreate(in value) is { } inline)
            return inline;

        return _constantData.Add(in value);
    }

    public AssemblyRefHandle AddReferencedAssembly(AssemblyIdentity identity)
    {
        var data = ConvertAssemblyIdentity(identity);
        var id = AssemblyRefHandle.FromRowId(_referencedAssemblies.Count + 1);
        _referencedAssemblies.Add(data);
        return id;
    }

    public NamespaceDefHandle AddNamespaceDef(NamespaceDefRow defRow)
    {
        _namespaceDefs.Add(defRow);
        return NamespaceDefHandle.FromRowId(_namespaceDefs.Count);
    }

    public NamespaceRefHandle AddNamespaceRef(NamespaceRefRow refRow)
    {
        _namespaceRefs.Add(refRow);
        return NamespaceRefHandle.FromRowId(_namespaceRefs.Count);
    }

    public TypeDefHandle AddTypeDef(TypeDefinitionRow row)
    {
        _typeDefs.Add(row);
        return TypeDefHandle.FromRowId(_typeDefs.Count);
    }

    public TypeRefHandle AddTypeRef(TypeReferenceRow row)
    {
        _typeRefs.Add(row);
        return TypeRefHandle.FromRowId(_typeRefs.Count);
    }

    public TypeSpecHandle AddTypeSpec(TypeSpecRow row)
    {
        return _typeSpecHeap.Add(row);
    }

    public VariableHandle AddVariable(VariableRow row)
    {
        _variables.Add(row);
        return VariableHandle.FromRowId(_variables.Count);
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
            _namespaceDefs,
            sectionHeaders,
            Section.NamespaceDefs,
            ref currentSection
        );
        WriteRows(
            ref writer,
            ref bufferWriter,
            _namespaceRefs,
            sectionHeaders,
            Section.NamespaceRefs,
            ref currentSection
        );
        WriteRows(
            ref writer,
            ref bufferWriter,
            _typeDefs,
            sectionHeaders,
            Section.TypeDefs,
            ref currentSection
        );
        WriteRows(
            ref writer,
            ref bufferWriter,
            _typeRefs,
            sectionHeaders,
            Section.TypeRefs,
            ref currentSection
        );
        WriteHeap(
            ref writer,
            ref bufferWriter,
            _typeSpecHeap,
            sectionHeaders,
            Section.TypeSpecs,
            ref currentSection
        );
        WriteRows(
            ref writer,
            ref bufferWriter,
            _variables,
            sectionHeaders,
            Section.Variables,
            ref currentSection
        );
        WriteHeap(
            ref writer,
            ref bufferWriter,
            _constantData,
            sectionHeaders,
            Section.Constants,
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
