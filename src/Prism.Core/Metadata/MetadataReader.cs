using System.Buffers;
using System.Diagnostics;
using MagicArchive;
using ZLinq;

namespace Prism.Core.Metadata;

internal sealed class MetadataReader
{
    private readonly StringHeapReader _stringHeap;
    private readonly BlobHeapReader _blobHeap;
    private readonly TableRowReader<AssemblyIdentityData> _assemblyRefs;
    private readonly TableRowReader<NamespaceDefRow> _namespaceDefs;
    private readonly TableRowReader<NamespaceRefRow> _namespaceRefs;
    private readonly TableRowReader<TypeDefinitionRow> _typeDefs;
    private readonly TableRowReader<TypeReferenceRow> _typeRefs;
    private readonly TypeSpecHeapReader _typeSpecs;
    private readonly TableRowReader<VariableRow> _variables;
    private readonly TableRowReader<FunctionRow> _functions;
    private readonly TableRowReader<ParameterRow> _parameters;
    private readonly TableRowReader<CustomAttributeRow> _customAttributes;

    public MetadataReader(ReadOnlyMemory<byte> buffer)
    {
        using var archiveReaderState = ArchiveReaderStatePool.Rent(
            ArchiveSerializerOptions.LittleEndian
        );
        SectionHeader[]? rentedHeaders = null;
        try
        {
            var reader = new ArchiveReader(buffer.Span, archiveReaderState);
            Span<SectionHeader> sectionHeaders = stackalloc SectionHeader[Sections.Length];
            if (!reader.TryReadInto(sectionHeaders, out var length))
            {
                rentedHeaders = ArrayPool<SectionHeader>.Shared.Rent(length);
                sectionHeaders = rentedHeaders.AsSpan(0, length);
            }
            var read = reader.TryReadInto(sectionHeaders, out _);
            Debug.Assert(read);

            var lookup = sectionHeaders.AsValueEnumerable().ToDictionary(x => x.Kind);
            _stringHeap = CreateHeapReader<StringHeapReader>(
                buffer,
                lookup.GetValueOrDefault(Section.StringTable)
            );
            _blobHeap = CreateHeapReader<BlobHeapReader>(
                buffer,
                lookup.GetValueOrDefault(Section.Blobs)
            );
            _assemblyRefs = CreateTableRowReader<AssemblyIdentityData>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.AssemblyReferences)
            );
            _namespaceDefs = CreateTableRowReader<NamespaceDefRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.NamespaceDefs)
            );
            _namespaceRefs = CreateTableRowReader<NamespaceRefRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.NamespaceRefs)
            );
            _typeDefs = CreateTableRowReader<TypeDefinitionRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.TypeDefs)
            );
            _typeRefs = CreateTableRowReader<TypeReferenceRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.TypeRefs)
            );
            _typeSpecs = CreateHeapReader<TypeSpecHeapReader>(
                buffer,
                lookup.GetValueOrDefault(Section.TypeSpecs)
            );
            _variables = CreateTableRowReader<VariableRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.Variables)
            );
            _functions = CreateTableRowReader<FunctionRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.Functions)
            );
            _parameters = CreateTableRowReader<ParameterRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.Parameters)
            );
            _customAttributes = CreateTableRowReader<CustomAttributeRow>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.CustomAttributes)
            );
        }
        finally
        {
            if (rentedHeaders is not null)
                ArrayPool<SectionHeader>.Shared.Return(rentedHeaders);
        }
    }

    public string GetString(StringHandle handle)
    {
        return _stringHeap.GetValue(handle.Id);
    }

    internal T GetBlob<T>(BlobHandle handle)
    {
        return _blobHeap.GetValue<T>(handle.Id);
    }

    private static T CreateHeapReader<T>(ReadOnlyMemory<byte> buffer, SectionHeader section)
        where T : IHeapReader<T>
    {
        return T.Create(buffer.Slice(section.Offset, section.Size), section.Count);
    }

    private static TableRowReader<T> CreateTableRowReader<T>(
        ReadOnlyMemory<byte> buffer,
        ArchiveReaderState readerState,
        SectionHeader section
    )
        where T : unmanaged
    {
        return new TableRowReader<T>(
            buffer.Slice(section.Offset, section.Size),
            readerState,
            section.Count
        );
    }
}
