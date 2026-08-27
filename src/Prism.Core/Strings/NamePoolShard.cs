// @file NamePoolShard.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Numerics;
using Prism.Core.Utils;

namespace Prism.Core.Strings;

internal sealed class NamePoolShard(NameEntryList entries)
{
    private const int LoadFactorQuotient = 9;
    private const int LoadFactorDivisor = 10;

    private readonly ReaderWriterLockSlim _lock = new();
    private int _usedSlots = 0;
    private NameSlot[] _slots = new NameSlot[NamePool.InitialSlotsPerShard];
    private readonly NameEntryList _entries = entries;
    private int _numCreatedEntries = 0;

    public int Capacity => _slots.Length;

    public int NumCreatedEntries => _numCreatedEntries;

    public NameEntryId Find(in NameValue value)
    {
        using var scope = _lock.EnterReadScope();
        var slot = Probe(in value);
        return slot.Id;
    }

    public NameEntryId Insert(in NameValue value)
    {
        using var scope = _lock.EnterWriteScope();
        ref var slot = ref Probe(in value);
        if (slot.Used)
            return slot.Id;

        return CreateAndInsertEntry(ref slot, value);
    }

    public void Reserve(int count)
    {
        var wantedCapacity =
            (int)BitOperations.RoundUpToPowerOf2((uint)count + 1)
            * LoadFactorDivisor
            / LoadFactorQuotient;

        using var scope = _lock.EnterWriteScope();
        if (wantedCapacity <= Capacity)
            return;

        if (wantedCapacity > Capacity)
        {
            Grow(wantedCapacity);
        }
    }

    private ref struct NameValueContext
    {
        public required NameValue Value { get; init; }
        public required NameEntryList Entries { get; init; }
    }

    private static bool EntryEqualsValue(string entry, NameValue value)
    {
        return entry.Length == value.Hash.Length
            && entry.Equals(value.Name, StringComparison.Ordinal);
    }

    // ReSharper disable once RedundantAssignment
    private void ClaimSlot(ref NameSlot unusedSlot, NameSlot newValue)
    {
        Debug.Assert(!unusedSlot.Used);

        unusedSlot = newValue;
        ++_usedSlots;
        if (_usedSlots * LoadFactorDivisor > LoadFactorQuotient * Capacity)
        {
            Grow();
        }
    }

    private NameEntryId CreateAndInsertEntry(ref NameSlot slot, in NameValue value)
    {
        var newEntryId = _entries.Create(value.Name);

        ClaimSlot(ref slot, new NameSlot(newEntryId, value.Hash.SlotProbeHash));
        Interlocked.Increment(ref _usedSlots);
        return newEntryId;
    }

    private void Grow()
    {
        Grow(Capacity * 2);
    }

    private void Grow(int newCapacity)
    {
        var oldSlots = _slots;
        var oldUsedSlots = _usedSlots;

        _slots = new NameSlot[newCapacity];
        _usedSlots = 0;

        foreach (var oldSlot in oldSlots)
        {
            if (!oldSlot.Used)
                continue;

            RehashAndInsert(oldSlot);
        }

        Debug.Assert(_usedSlots == oldUsedSlots);
    }

    private ref NameSlot Probe(in NameValue value)
    {
        var ctx = new NameValueContext { Value = value, Entries = _entries };
        return ref Probe(
            value.Hash.UnmaskedSlotIndex,
            in ctx,
            static (slot, in c) =>
                slot.ProbeHash == c.Value.Hash.SlotProbeHash
                && EntryEqualsValue(c.Entries.Resolve(slot.Id), c.Value)
        );
    }

    private delegate bool ProbePredicate<TContext>(NameSlot slot, scoped in TContext context)
        where TContext : allows ref struct;

    private ref NameSlot Probe<TContext>(
        uint unmaskedSlotIndex,
        scoped in TContext context,
        ProbePredicate<TContext> predicate
    )
        where TContext : allows ref struct
    {
        var mask = (uint)Capacity - 1;
        for (var i = NameHash.GetProbeStart(unmaskedSlotIndex, mask); ; i = i + 1 & mask)
        {
            ref var slot = ref _slots[i];
            if (!slot.Used || predicate(slot, in context))
            {
                return ref slot;
            }
        }
    }

    private void RehashAndInsert(NameSlot oldSlot)
    {
        Debug.Assert(oldSlot.Used);

        var name = _entries.Resolve(oldSlot.Id);
        var hash = new NameHash(name);
        ref var newSlot = ref Probe(hash.UnmaskedSlotIndex, 0, static (_, in _) => false);
        newSlot = oldSlot;
        _usedSlots++;
    }
}
