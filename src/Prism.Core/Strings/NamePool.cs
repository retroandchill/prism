// @file NamePool.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Core.Strings;

[InlineArray((int)Names.PoolShards)]
internal struct NamePoolShardList
{
    private NamePoolShard _element0;
}

internal sealed class NamePool
{
    private readonly NameEntryList _entries = new();
    private NamePoolShardList _shards;

    public uint NumEntries
    {
        get
        {
            var value = 0u;
            foreach (var shard in _shards)
            {
                value += shard.NumCreatedEntries;
            }

            return value;
        }
    }

    public uint NumBlocks => _entries.NumBlocks;

    public uint NumSlots
    {
        get
        {
            var slotCapacity = 0u;
            foreach (var shard in _shards)
            {
                slotCapacity += shard.Capacity;
            }

            return slotCapacity;
        }
    }

    public static NamePool Instance { get; } = new();

    private NamePool()
    {
        foreach (ref var shard in _shards)
        {
            shard = new NamePoolShard(_entries);
        }

        Store("");
    }

    public void Reserve(uint numBlocks, uint numEntries)
    {
        _entries.ReserveBlocks(numBlocks);

        if (NumEntries >= numEntries)
            return;
        var numEntriesPerShard = numEntries / Names.PoolShards + 1;

        foreach (var shard in _shards)
        {
            shard.Reserve(numEntriesPerShard);
        }
    }

    public NameEntryId Store(ReadOnlySpan<char> text)
    {
        var value = new NameValue(text);
        var (id, _) = _shards[(int)value.Hash.ShardIndex].Insert(value);
        return id;
    }

    public NameEntryId Find(ReadOnlySpan<char> text)
    {
        var value = new NameValue(text);
        return _shards[(int)value.Hash.ShardIndex].Find(value);
    }

    public string Resolve(NameEntryHandle handle)
    {
        return _entries.Resolve(handle);
    }

    public bool IsValid(NameEntryHandle handle)
    {
        return handle.Block < _entries.NumBlocks;
    }
}
