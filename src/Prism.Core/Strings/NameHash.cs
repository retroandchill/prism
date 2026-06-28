// @file NameHash.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Hashing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Prism.Core.Strings;

internal readonly struct NameHash
    : IEquatable<NameHash>,
        IEqualityOperators<NameHash, NameHash, bool>
{
    public uint ShardIndex { get; }
    public uint UnmaskedSlotIndex { get; }
    public uint SlotProbeHash { get; }

    private const uint ShardMask = Names.PoolShards - 1;

    public NameHash(ReadOnlySpan<char> str)
        : this(GenerateHash(str), str.IsEmpty) { }

    private NameHash(ulong hash, bool isNone)
    {
        var hi = (uint)(hash >> 32);
        var lo = (uint)hash;

        var isNoneBit = (uint)(isNone ? 1 : 0) << (int)NameSlot.ProbeHashShift;

        ShardIndex = hi & ShardMask;
        UnmaskedSlotIndex = lo;
        SlotProbeHash = (hi & NameSlot.ProbeHashMask) | isNoneBit;
    }

    private static ulong GenerateHash(ReadOnlySpan<char> str)
    {
        return XxHash3.HashToUInt64(MemoryMarshal.AsBytes(str));
    }

    public static uint GetProbeStart(uint unmaskedSlotIndex, uint slotMask)
    {
        return unmaskedSlotIndex & slotMask;
    }

    public override bool Equals(object? obj)
    {
        return obj is NameHash other && Equals(other);
    }

    public bool Equals(NameHash other)
    {
        return ShardIndex == other.ShardIndex
            && UnmaskedSlotIndex == other.UnmaskedSlotIndex
            && SlotProbeHash == other.SlotProbeHash;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ShardIndex, UnmaskedSlotIndex, SlotProbeHash);
    }

    public static bool operator ==(NameHash left, NameHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NameHash left, NameHash right)
    {
        return !(left == right);
    }
}
