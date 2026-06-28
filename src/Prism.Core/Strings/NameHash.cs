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
    public ushort Length { get; }

    public const ulong AlgorithmId = 0xC1640000;
    public const uint ShardMask = Names.PoolShards - 1;

    public NameHash(ReadOnlySpan<char> str)
        : this(GenerateHash(str), str.Length, str.IsEmpty) { }

    public NameHash(ReadOnlySpan<char> str, ulong hash)
        : this(hash, str.Length, str.IsEmpty) { }

    public NameHash(ulong hash, int length, bool isNone)
    {
        var hi = (uint)(hash >> 32);
        var lo = (uint)hash;

        var isNoneBit = (uint)(isNone ? 1 : 0) << (int)NameSlot.ProbeHashShift;

        ShardIndex = hi & ShardMask;
        UnmaskedSlotIndex = lo;
        SlotProbeHash = (hi & NameSlot.ProbeHashMask) | isNoneBit;
        Length = (ushort)length;
    }

    public static uint GetShardIndex(ulong hash)
    {
        return (uint)(hash >> 32) & ShardMask;
    }

    public static ulong GenerateHash(ReadOnlySpan<char> str)
    {
        return XxHash3.HashToUInt64(MemoryMarshal.AsBytes(str));
    }

    public uint GetProbeStart(uint slotMask)
    {
        return UnmaskedSlotIndex & slotMask;
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
            && SlotProbeHash == other.SlotProbeHash
            && Length == other.Length;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ShardIndex, UnmaskedSlotIndex, SlotProbeHash, Length);
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
