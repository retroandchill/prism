// @file NameSlot.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Prism.Core.Strings;

internal readonly struct NameSlot(NameEntryId id, uint probeHash)
    : IEquatable<NameSlot>,
        IEqualityOperators<NameSlot, NameSlot, bool>
{
    public const uint EntryIdBits = Names.MaxBlockBits + Names.BlockOffsetBits;
    public const uint EntryIdMask = (1 << (int)EntryIdBits) - 1;
    public const uint ProbeHashShift = EntryIdBits;
    public const uint ProbeHashMask = ~EntryIdMask;

    private readonly uint _idAndHash = id.ToUnstableInt() | probeHash;

    public NameEntryId Id => NameEntryId.FromUnstableInt(_idAndHash & EntryIdMask);

    public uint ProbeHash => _idAndHash & ProbeHashMask;

    public bool Used => _idAndHash != 0;

    public override bool Equals(object? obj) => obj is NameSlot other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(NameSlot other)
    {
        return _idAndHash == other._idAndHash;
    }

    public override int GetHashCode()
    {
        return (int)_idAndHash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NameSlot left, NameSlot right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NameSlot left, NameSlot right)
    {
        return !(left == right);
    }
}
