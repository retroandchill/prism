// @file NamePoolShard.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using Prism.Core.Utils;

namespace Prism.Core.Strings;

internal delegate bool NameSlotPredicate<TContext>(scoped in TContext context, NameSlot slot)
    where TContext : allows ref struct;

internal sealed class NamePoolShard(NameEntryList entries)
{
    private const uint LoadFactoryQuotient = 9;
    private const uint LoadFactorDivisor = 10;

    private readonly ReaderWriterLockSlim _lock = new();
    private uint _usedSlots;
    private uint _capacityMask = Names.PoolInitialSLotsPerShard - 1;
    private NameSlot[] _slots = new NameSlot[Names.PoolInitialSLotsPerShard];
    private uint _numCreatedEntries;

    public uint Capacity => _capacityMask + 1;
    public uint NumCreatedEntries => _numCreatedEntries;

    public NameEntryId Find(in NameValue value)
    {
        using var scope = _lock.EnterReadScope();
        var slot = Probe(value);
        return slot.Id;
    }

    public NameEntryId Insert(in NameValue value)
    {
        using var scope = _lock.EnterWriteScope();
        ref var slot = ref Probe(value);

        return slot.Used ? slot.Id : CreateAndInsertEntry(ref slot, value);
    }

    public void Reserve(uint numSlots)
    {
        var wantedCapacity = BitOperations.RoundUpToPowerOf2(numSlots);

        using var scope = _lock.EnterWriteScope();
        if (wantedCapacity > Capacity)
        {
            Grow(wantedCapacity);
        }
    }

    // ReSharper disable once RedundantAssignment
    private void ClaimSlot(ref NameSlot unusedSlot, NameSlot newValue)
    {
        unusedSlot = newValue;

        ++_usedSlots;
        if (_usedSlots * LoadFactorDivisor > LoadFactoryQuotient * Capacity)
        {
            Grow();
        }
    }

    private NameEntryId CreateAndInsertEntry(ref NameSlot slot, in NameValue value)
    {
        var newEntryId = entries.Create(value.Name.ToString());

        ClaimSlot(ref slot, new NameSlot(newEntryId, value.Hash.SlotProbeHash));
        Interlocked.Increment(ref _numCreatedEntries);
        return newEntryId;
    }

    private void Grow()
    {
        Grow(Capacity * 2);
    }

    private void Grow(uint newCapacity)
    {
        var oldSlots = _slots.AsSpan((int)Capacity);

        _slots = new NameSlot[newCapacity];
        _usedSlots = 0;
        _capacityMask = newCapacity - 1;

        const int prefetchDepth = 8;
        Span<NameSlot> prefetchedSlots = stackalloc NameSlot[prefetchDepth];
        var numPrefetched = 0;

        foreach (var oldSlot in oldSlots)
        {
            if (!oldSlot.Used)
                continue;

            prefetchedSlots[numPrefetched] = oldSlot;

            if (++numPrefetched != prefetchDepth)
                continue;
            foreach (var prefetchedSlot in prefetchedSlots)
            {
                RehashAndInsert(prefetchedSlot);
            }

            numPrefetched = 0;
        }

        foreach (var prefetchedSlot in prefetchedSlots[..numPrefetched])
        {
            RehashAndInsert(prefetchedSlot);
        }
    }

    private ref NameSlot Probe(in NameValue value)
    {
        return ref Probe(
            value.Hash.UnmaskedSlotIndex,
            in value,
            (in val, slot) =>
                slot.ProbeHash == val.Hash.SlotProbeHash
                && EntryEqualsValue(entries.Resolve(slot.Id), in val)
        );
    }

    private ref NameSlot Probe<TContext>(
        uint unmaskedSlotIndex,
        scoped in TContext context,
        NameSlotPredicate<TContext> predicate
    )
        where TContext : allows ref struct
    {
        var mask = _capacityMask;
        for (var i = NameHash.GetProbeStart(unmaskedSlotIndex, mask); ; i = (i + 1) & mask)
        {
            ref var slot = ref _slots[i];
            if (!slot.Used || predicate(in context, slot))
            {
                return ref slot;
            }
        }
    }

    private void RehashAndInsert(NameSlot oldSlot)
    {
        var entry = entries.Resolve(oldSlot.Id);
        var hash = new NameHash(entry);
        ref var newSlot = ref Probe(hash.UnmaskedSlotIndex, 0, (in _, _) => false);
        newSlot = oldSlot;
        ++_usedSlots;
    }

    private static bool EntryEqualsValue(string str, in NameValue value)
    {
        return value.Name.Equals(str, StringComparison.Ordinal);
    }
}
