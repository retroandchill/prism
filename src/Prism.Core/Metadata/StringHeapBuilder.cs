// @file StringHeapBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;
using MagicArchive;

namespace Prism.Core.Metadata;

[MustDisposeResource]
internal sealed class StringHeapBuilder : IHeapBuilder, IDisposable
{
    private readonly Dictionary<string, StringHandle> _map = new(StringComparer.Ordinal);

    private bool _disposed;
    private readonly ArchiveWriterState _state = ArchiveWriterStatePool.Rent(
        ArchiveSerializerOptions.LittleEndian
    );
    private MetadataBuffer _buffer = new();

    public ReadOnlySpan<byte> Data => _buffer.WrittenSpan;

    public int Count => _map.Count;

    public StringHandle GetOrAdd(string value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (KnownString.TryGet(value) is { } known)
        {
            return known.ToHandle();
        }

        if (_map.TryGetValue(value, out var handle))
            return handle;

        if (int.MaxValue - _buffer.Position < KnownStrings.HandleShift)
            throw new InvalidOperationException("Too many strings have been interned");

        handle = StringHandle.FromRowId(_buffer.Position + KnownStrings.HandleShift);
        var writer = ArchiveWriter.Create(ref _buffer, _state);
        writer.WriteMetadataString(value);
        _map.Add(value, handle);
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
}
