using System.Buffers;
using System.Collections.Concurrent;
using MagicArchive;
using MagicArchive.Formatters;
using MagicArchive.Utilities;

namespace Prism.Core.Metadata;

using ExtraMapping = ConcurrentDictionary<ArchiveWriterState, MetadataWriteState>;

internal sealed class MetadataWriteState : IDisposable
{
    internal static readonly ExtraMapping Mapping = new(ReferenceEqualityComparer.Instance);

    private readonly ArchiveWriterState _writerState;
    private readonly Dictionary<string, int> _stringTableMapping = new();
    private ReusableLinkedArrayBufferWriter _stringTable;

    public MetadataWriteState(ArchiveWriterState state)
    {
        _writerState = state;
        _stringTable = ReusableLinkedArrayBufferWriterPool.Rent();
        Mapping.TryAdd(_writerState, this);
    }

    public int GetOrAddString(string str)
    {
        if (_stringTableMapping.TryGetValue(str, out var offset))
            return offset;

        offset = _stringTable.TotalWritten;
        var writer = ArchiveWriter.Create(ref _stringTable, _writerState);
        Utf8StringFormatter.Default.Serialize(ref writer, str);
        _stringTableMapping.Add(str, offset);
        return offset;
    }

    public void WriteToTable<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.WriteBlittable(_stringTable.TotalWritten);
        _stringTable.WriteToAndReset(ref writer);
    }

    public void Dispose()
    {
        ReusableLinkedArrayBufferWriterPool.Return(_stringTable);
    }
}

internal static class MetadataWriteStateExtensions
{
    extension<TBufferWriter>(in ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        public MetadataWriteState? MetadataState =>
            MetadataWriteState.Mapping.GetValueOrDefault(writer.State);
    }
}
