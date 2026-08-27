// @file NameSlot.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Prism.Core.Strings;

public readonly struct NameSlot : IEquatable<NameSlot>, IEqualityOperators<NameSlot, NameSlot, bool>
{
    public const int EntryIdBits = Name.MaxBlockBits + Name.BlockOffsetBits;
    public const uint EntryIdMask = (1u << EntryIdBits) - 1;
    public const int ProbeHashShift = EntryIdBits;
    public const uint ProbeHashMask = ~EntryIdMask;

    private readonly uint _idAndHash;

    public NameEntryId Id => NameEntryId.FromUnstableInt(_idAndHash & EntryIdMask);

    public uint ProbeHash => _idAndHash & ProbeHashMask;

    public bool Used => _idAndHash != 0;

    public NameSlot(NameEntryId value, uint probeHash)
    {
        _idAndHash = value.ToUnstableInt() | probeHash;
        Debug.Assert(
            (value.ToUnstableInt() & ProbeHashMask) == 0 && (probeHash & EntryIdMask) == 0 && Used
        );
    }

    public override bool Equals(object? obj)
    {
        return obj is NameSlot other && Equals(other);
    }

    public bool Equals(NameSlot other)
    {
        return _idAndHash == other._idAndHash;
    }

    public override int GetHashCode()
    {
        return _idAndHash.GetHashCode();
    }

    public static bool operator ==(NameSlot left, NameSlot right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NameSlot left, NameSlot right)
    {
        return !(left == right);
    }
}
