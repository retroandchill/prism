// @file BlobHeapBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class BlobHeapBuilder : IDisposable
{
    private readonly Dictionary<ImmutableArray<byte>, BlobHandle> _map = new(
        BinarySequenceComparer.Default
    );

    private bool _disposed;
    private readonly ArchiveWriterState _state = ArchiveWriterStatePool.Rent(
        ArchiveSerializerOptions.LittleEndian
    );
    private MetadataBuffer _buffer = new();

    public ReadOnlySpan<byte> Data => _buffer.WrittenSpan;

    public int Count => _map.Count;

    public BlobHandle GetOrAdd(ReadOnlySpan<byte> value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var lookup = _map.GetAlternateLookup<ReadOnlySpan<byte>>();
        if (lookup.TryGetValue(value, out var handle))
            return handle;

        handle = new BlobHandle(_buffer.Position);
        var writer = ArchiveWriter.Create(ref _buffer, _state);
        writer.WriteSpan(value);
        lookup.TryAdd([.. value], handle);
        writer.Flush();
        return handle;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ((IDisposable)_state).Dispose();
    }

    private sealed class BinarySequenceComparer
        : IEqualityComparer<ImmutableArray<byte>>,
            IAlternateEqualityComparer<ReadOnlySpan<byte>, ImmutableArray<byte>>
    {
        public static readonly BinarySequenceComparer Default = new();

        public bool Equals(ImmutableArray<byte> x, ImmutableArray<byte> y)
        {
            return x.SequenceEqual(y);
        }

        public bool Equals(ReadOnlySpan<byte> alternate, ImmutableArray<byte> other)
        {
            return alternate.SequenceEqual(other.AsSpan());
        }

        public ImmutableArray<byte> Create(ReadOnlySpan<byte> alternate)
        {
            return [.. alternate];
        }

        public int GetHashCode(ImmutableArray<byte> obj)
        {
            return GetHashCode(obj.AsSpan());
        }

        public int GetHashCode(ReadOnlySpan<byte> alternate)
        {
            var hashCode = new HashCode();
            hashCode.AddBytes(alternate);
            return hashCode.ToHashCode();
        }
    }
}
