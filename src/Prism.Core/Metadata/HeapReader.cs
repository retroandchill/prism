// @file HeapReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using MagicArchive;

namespace Prism.Core.Metadata;

internal interface IHeapReader<out TSelf>
{
    static abstract TSelf Create(ReadOnlyMemory<byte> buffer, int totalItems);
}

internal abstract class HeapReader<T>(ReadOnlyMemory<byte> buffer, int totalItems, int indexShift)
{
    private ReadOnlyMemory<byte> _buffer = buffer;
    private int _remaining = totalItems;

    private readonly ConcurrentDictionary<int, Lazy<T>> _cache = new();

    public T GetValue(int id)
    {
        var lazy = _cache.GetOrAdd(
            id,
            static (h, @this) =>
                new Lazy<T>(() => @this.ReadValue(h), LazyThreadSafetyMode.ExecutionAndPublication),
            this
        );

        return lazy.Value;
    }

    private T ReadValue(int id)
    {
        ArgumentOutOfRangeException.ThrowIfZero(id);
        // Special cases don't count against the remaining items
        // because they're usually values that are built into the compiler
        var start = id - indexShift;
        if (start < 0)
            return ReadSpecialCase(id);

        var span = _buffer.Span[start..];
        using var state = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var reader = new ArchiveReader(span, state);
        var value = Read(ref reader);

        if (Interlocked.Decrement(ref _remaining) == 0)
            _buffer = default;

        return value;
    }

    protected virtual T ReadSpecialCase(int id)
    {
        throw new InvalidOperationException($"Cannot read special case for id: {id}");
    }

    protected abstract T Read(ref ArchiveReader reader);
}
