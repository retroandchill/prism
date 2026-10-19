using System.Buffers;
using System.Collections.Concurrent;
using MagicArchive;
using Microsoft.Extensions.ObjectPool;

namespace Prism.Core.Metadata;

using ExtraMapping = ConcurrentDictionary<ArchiveWriterState, MetadataWriteState>;

internal sealed class MetadataWriteState : IDisposable
{
    private static readonly ObjectPool<MetadataWriteState> Pool;
    internal static readonly ExtraMapping Mapping = new(ReferenceEqualityComparer.Instance);

    private ArchiveWriterState _writerState = null!;

    private MetadataWriteState() { }

    public static MetadataWriteState Create(ArchiveWriterState state)
    {
        var result = Pool.Get();
        result.Init(state);
        return result;
    }

    private void Init(ArchiveWriterState state)
    {
        _writerState = state;
        Mapping.TryAdd(_writerState, this);
    }

    public Dictionary<string, int> StringTable { get; } = new();

    private sealed class ContextObjectPolicy : IPooledObjectPolicy<MetadataWriteState>
    {
        public MetadataWriteState Create()
        {
            return new MetadataWriteState();
        }

        public bool Return(MetadataWriteState obj)
        {
            Mapping.TryRemove(obj._writerState, out _);
            obj._writerState = null!;
            obj.StringTable.Clear();
            return true;
        }
    }

    static MetadataWriteState()
    {
        var provider = new DefaultObjectPoolProvider();
        Pool = provider.Create(new ContextObjectPolicy());
    }

    public void Dispose()
    {
        Pool.Return(this);
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
