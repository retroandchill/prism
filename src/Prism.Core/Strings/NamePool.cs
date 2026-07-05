// @file NamePool.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ZLinq;

namespace Prism.Core.Strings;

internal sealed class NamePool
{
    public const int ShardBits = 10;
    public const int Shards = 1 << ShardBits;
    public const int InitialSlotBits = 8;
    public const int InitialSlotsPerShard = 1 << InitialSlotBits;

    private readonly NameEntryList _entries = new();
    private readonly ImmutableArray<NamePoolShard> _shards;
    private readonly ImmutableArray<NameEntryId> _knownNames;
    private uint _largestKnownNameUnstableId;
    private Dictionary<NameEntryId, KnownName> _entryToKnownName = new();

    public int NumEntries
    {
        get { return _shards.AsValueEnumerable().Sum(s => s.NumCreatedEntries); }
    }

    public int NumBlocks => _entries.NumBlocks;

    public int NumSlots
    {
        get { return _shards.AsValueEnumerable().Sum(s => s.Capacity); }
    }

    private NamePool()
    {
        var shards = new NamePoolShard[Shards];
        foreach (ref var shard in shards.AsSpan())
        {
            shard = new NamePoolShard(_entries);
        }
        _shards = ImmutableCollectionsMarshal.AsImmutableArray(shards);

        _entryToKnownName.EnsureCapacity(KnownNames.Length);
        var knownNames = new NameEntryId[KnownNames.Length];
        for (var i = 0; i < KnownNames.Length; i++)
        {
            var knownName = (KnownName)i;
            knownNames[i] = Store(knownName.ToStringFast(true));

            if (i != KnownName.None.AsUnderlyingType() && knownNames[i].IsNone)
                continue;

            _entryToKnownName.Add(knownNames[i], knownName);
            _largestKnownNameUnstableId = Math.Max(
                _largestKnownNameUnstableId,
                knownNames[i].ToUnstableInt()
            );
        }
        _knownNames = ImmutableCollectionsMarshal.AsImmutableArray(knownNames);
    }

    public static NamePool Instance { get; } = new();

    public void Reserve(int numBlocks, int entries)
    {
        _entries.ReserveBlocks(numBlocks);

        if (NumEntries >= entries)
            return;
        var numEntriesPerShard = NumEntries / Shards + 1;
        foreach (var shard in _shards)
        {
            shard.Reserve(numEntriesPerShard);
        }
    }

    public NameEntryId Store(ReadOnlySpan<char> name)
    {
        var value = new NameValue(name);
        return _shards[value.Hash.ShardIndex].Insert(value);
    }

    public NameEntryId Find(ReadOnlySpan<char> name)
    {
        var value = new NameValue(name);
        return _shards[value.Hash.ShardIndex].Find(value);
    }

    public NameEntryId Find(KnownName name)
    {
        Debug.Assert(name.AsUnderlyingType() < KnownNames.Length);
        return _knownNames[name.AsUnderlyingType()];
    }

    public KnownName? FindKnown(NameEntryId id)
    {
        return id.ToUnstableInt() <= KnownNames.Length ? _entryToKnownName[id] : null;
    }

    public string Resolve(NameEntryHandle handle)
    {
        return _entries.Resolve(handle);
    }
}
