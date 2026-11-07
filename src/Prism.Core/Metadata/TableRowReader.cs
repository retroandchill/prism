// @file TableRowReader.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagicArchive;

namespace Prism.Core.Metadata;

internal delegate T ReadMetadataRow<TRow, out T>(int id, in TRow row)
    where TRow : unmanaged;

internal sealed class TableRowReader<TRow, T>
    where TRow : unmanaged
{
    private readonly RowBackingState<TRow> _backingState;

    private readonly ReadMetadataRow<TRow, T> _readRow;
    private readonly ConcurrentDictionary<int, Lazy<T>> _cache = new();

    public TableRowReader(ImmutableArray<TRow> buffer, ReadMetadataRow<TRow, T> readRow)
    {
        _backingState = new RowBackingState<TRow>(buffer);
        _readRow = readRow;
        Count = buffer.Length;
    }

    public RegisteredLazy<TValue> RegisterAdditionalWork<TValue>(
        Func<ReadOnlySpan<TRow>, TValue> factory
    )
        where TValue : class
    {
        return _backingState.RegisterLazy(factory);
    }

    public int Count { get; }

    public T GetRow(int rowId)
    {
        var lazy = _cache.GetOrAdd(
            rowId,
            static (h, @this) =>
                new Lazy<T>(() => @this.ReadValue(h), LazyThreadSafetyMode.ExecutionAndPublication),
            this
        );

        return lazy.Value;
    }

    private T ReadValue(int rowId)
    {
        return _backingState.MaterializeRow(rowId, _readRow);
    }
}
