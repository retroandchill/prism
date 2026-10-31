// @file BlobHeapReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using MagicArchive;

namespace Prism.Core.Metadata;

public class BlobHeapReader(ReadOnlyMemory<byte> buffer, int totalItems)
    : IHeapReader<BlobHeapReader>
{
    private ReadOnlyMemory<byte> _buffer = buffer;
    private int _remaining = totalItems;

    private readonly ConcurrentDictionary<int, object> _cache = new();

    public T GetValue<T>(int offset)
    {
        var cached = _cache.GetOrAdd(
            offset,
            static (h, @this) =>
                new Lazy<T>(
                    () => @this.ReadValue<T>(h),
                    LazyThreadSafetyMode.ExecutionAndPublication
                ),
            this
        );

        return cached is Lazy<T> lazy
            ? lazy.Value
            : throw new InvalidOperationException(
                $"Value at offset {offset} was previously deserialized as a different type"
            );
    }

    private T ReadValue<T>(int id)
    {
        ArgumentOutOfRangeException.ThrowIfZero(id);
        // Special cases don't count against the remaining items
        // because they're usually values that are built into the compiler
        var start = id - BlobMetadata.HandleShift;
        if (start < 0)
            return ReadSpecialCase<T>(id);

        var span = _buffer.Span[start..];
        using var state = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var reader = new ArchiveReader(span, state);
        var value =
            reader.ReadValue<T>()
            ?? throw new InvalidOperationException("Cannot deserialize null out of blob heap");

        if (Interlocked.Decrement(ref _remaining) == 0)
            _buffer = default;

        return value;
    }

    static BlobHeapReader IHeapReader<BlobHeapReader>.Create(
        ReadOnlyMemory<byte> buffer,
        int totalItems
    )
    {
        return new BlobHeapReader(buffer, totalItems);
    }

    private static T ReadSpecialCase<T>(int id)
    {
        throw new InvalidOperationException("No special cases for blobs defined yet");
    }
}
