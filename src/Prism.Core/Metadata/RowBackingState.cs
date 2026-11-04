// @file RowBackingState.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal sealed class RowBackingState<TRow>(TRow[] rows)
    where TRow : unmanaged
{
    private TRow[]? _rows = rows;
    private int _activeLeases;
    private int _pendingRowMaterializations = rows.Length;
    private int _pendingRowDependentJobs;

    public int Count { get; } = rows.Length;

    public RegisteredLazy<TValue> RegisterLazy<TValue>(Func<ReadOnlySpan<TRow>, TValue> factory)
        where TValue : class
    {
        Interlocked.Increment(ref _pendingRowDependentJobs);
        return RegisteredLazy.Create(this, factory);
    }

    public TValue MaterializeRow<TValue>(int rowId, ReadMetadataRow<TRow, TValue> reader)
    {
        using var lease = AcquireLease();
        var rows =
            Volatile.Read(ref _rows) ?? throw new InvalidOperationException("Rows released.");
        var value = reader(rowId, in rows[rowId - 1]);
        Interlocked.Increment(ref _pendingRowMaterializations);
        MaterializeRow();
        return value;
    }

    internal TValue BuildLazyValue<TValue>(Func<ReadOnlySpan<TRow>, TValue> factory)
        where TValue : class
    {
        using var lease = AcquireLease();
        var rows =
            Volatile.Read(ref _rows) ?? throw new InvalidOperationException("Rows released.");
        var value = factory(rows);
        CompletePendingJob();
        return value;
    }

    private void CompletePendingJob()
    {
        if (Interlocked.Decrement(ref _pendingRowDependentJobs) == 0)
            TryReleaseRows();
    }

    private void MaterializeRow()
    {
        if (Interlocked.Decrement(ref _pendingRowMaterializations) == 0)
            CompletePendingJob();
    }

    public void TryReleaseWhenIdle()
    {
        TryReleaseRows();
    }

    private Lease AcquireLease()
    {
        Interlocked.Increment(ref _activeLeases);
        return new Lease(this);
    }

    private void ReleaseLease()
    {
        if (Interlocked.Decrement(ref _activeLeases) == 0)
            TryReleaseRows();
    }

    private void TryReleaseRows()
    {
        if (
            Volatile.Read(ref _activeLeases) == 0
            && Volatile.Read(ref _pendingRowMaterializations) == 0
            && Volatile.Read(ref _pendingRowDependentJobs) == 0
        )
        {
            _rows = null;
        }
    }

    private readonly struct Lease(RowBackingState<TRow> owner) : IDisposable
    {
        public void Dispose()
        {
            owner.ReleaseLease();
        }
    }
}
