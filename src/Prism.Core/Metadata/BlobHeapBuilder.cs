// @file BlobHeapBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;
using MagicArchive;

namespace Prism.Core.Metadata;

internal static class BlobMetadata
{
    public const int HandleShift = 512;
}

[MustDisposeResource]
internal sealed class BlobHeapBuilder : IHeapBuilder, IDisposable
{
    private bool _disposed;

    private readonly ArchiveWriterState _state = ArchiveWriterStatePool.Rent(
        ArchiveSerializerOptions.LittleEndian
    );

    private MetadataBuffer _buffer = new();

    public ReadOnlySpan<byte> Data => _buffer.WrittenSpan;

    public int Count { get; private set; }

    public BlobHandle Add<T>(in T value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (int.MaxValue - _buffer.Position < BlobMetadata.HandleShift)
            throw new InvalidOperationException("Too many blobs have been interned");

        var handle = BlobHandle.FromRowId(_buffer.Position + KnownStrings.HandleShift);
        var writer = ArchiveWriter.Create(ref _buffer, _state);
        writer.WriteValue(in value);
        Count++;
        return handle;
    }

    public BlobHandle Add<T>(scoped ReadOnlySpan<T> values)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (int.MaxValue - _buffer.Position < BlobMetadata.HandleShift)
            throw new InvalidOperationException("Too many blobs have been interned");

        var handle = BlobHandle.FromRowId(_buffer.Position + KnownStrings.HandleShift);
        var writer = ArchiveWriter.Create(ref _buffer, _state);
        writer.WriteSpan(values);
        Count++;
        return handle;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        ((IDisposable)_state).Dispose();
        _disposed = true;
    }
}
