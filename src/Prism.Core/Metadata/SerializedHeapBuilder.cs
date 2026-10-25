// @file BlobHeapBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MagicArchive;

namespace Prism.Core.Metadata;

internal sealed class SerializedHeapBuilder<THandle, TRow> : IHeapBuilder, IDisposable
    where THandle : struct, ITableRowHandle<THandle>
{
    private bool _disposed;
    private readonly ArchiveWriterState _state = ArchiveWriterStatePool.Rent(
        ArchiveSerializerOptions.LittleEndian
    );
    private MetadataBuffer _buffer = new();

    public ReadOnlySpan<byte> Data => _buffer.WrittenSpan;

    public int Count { get; private set; }

    public THandle Add(in TRow value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var handle = THandle.Create(_buffer.Position);
        var writer = ArchiveWriter.Create(ref _buffer, _state);
        writer.WriteValue(in value);
        writer.Flush();
        Count++;
        return handle;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ((IDisposable)_state).Dispose();
    }
}
