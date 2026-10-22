using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class MetadataReader : IDisposable
{
    private bool _disposed;
    private readonly ReadOnlySequence<byte> _buffer;
    private readonly ArchiveReaderState _archiveReaderState;
    private readonly MetadataReadState _metadataReadState;
    private readonly ImmutableArray<int> _sections;

    public MetadataReader(ReadOnlySequence<byte> buffer)
    {
        _archiveReaderState = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        try
        {
            var reader = new ArchiveReader(buffer, _archiveReaderState);
            _sections = reader.ReadValue<ImmutableArray<int>>();
            Debug.Assert(_sections.Length == Sections.Length);

            _buffer = buffer.Slice(reader.ConsumedBytes);

            var stringTableSection = GetSection(Section.StringTable);
            _metadataReadState = new MetadataReadState(stringTableSection, _archiveReaderState);
        }
        catch
        {
            ((IDisposable)_archiveReaderState).Dispose();
            throw;
        }
    }

    private ReadOnlySequence<byte> GetSection(Section section)
    {
        var index = (int)section;
        var start = _sections[index];
        if (index == _sections.Length - 1)
            return _buffer.Slice(start);

        var end = _sections[index + 1];
        return _buffer.Slice(start, end - start);
    }

    public ImmutableArray<AssemblyReference> ReadReferencedAssemblies()
    {
        var section = GetSection(Section.AssemblyReferences);
        if (section.IsEmpty)
            return [];

        var reader = new ArchiveReader(section, _archiveReaderState);
        return reader.ReadValue<ImmutableArray<AssemblyReference>>();
    }

    public ImmutableArray<MetadataNamespace> ReadNamespaces()
    {
        var section = GetSection(Section.Namespaces);
        if (section.IsEmpty)
            return [];

        var reader = new ArchiveReader(section, _archiveReaderState);
        return reader.ReadValue<ImmutableArray<MetadataNamespace>>();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _metadataReadState.Dispose();
        ((IDisposable)_archiveReaderState).Dispose();
    }
}
