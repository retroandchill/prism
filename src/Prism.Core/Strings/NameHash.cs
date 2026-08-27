// @file NameHash.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Hashing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Prism.Core.Strings;

public readonly struct NameHash : IEquatable<NameHash>, IEqualityOperators<NameHash, NameHash, bool>
{
    public const uint ShardMask = NamePool.Shards - 1;

    public int ShardIndex { get; }
    public uint UnmaskedSlotIndex { get; }
    public uint SlotProbeHash { get; }
    public int Length { get; }

    public NameHash(ReadOnlySpan<char> str)
        : this(GenerateHash(str), str.Length, IsNone(str)) { }

    public NameHash(ulong hash, int length, bool isNone)
    {
        var hi = (uint)(hash >> NamePool.ShardBits);
        var lo = (uint)hash;

        var isNoneBit = (uint)(isNone ? 1 : 0) << NameSlot.ProbeHashShift;

        ShardIndex = (int)(hi & ShardMask);
        UnmaskedSlotIndex = lo & NamePool.InitialSlotsPerShard - 1;
        SlotProbeHash = hi & NameSlot.ProbeHashMask | isNoneBit;
        Length = length;
    }

    public static ulong GenerateHash(ReadOnlySpan<char> str)
    {
        return XxHash3.HashToUInt64(MemoryMarshal.AsBytes(str));
    }

    public static uint GetProbeStart(uint unmaskedSlotIndex, uint slotMask)
    {
        return unmaskedSlotIndex & slotMask;
    }

    public static bool IsNone(ReadOnlySpan<char> str)
    {
        return str.Equals("<none>", StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is NameHash hash && Equals(hash);
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
