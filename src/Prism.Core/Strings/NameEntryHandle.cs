// @file NameEntryHandle.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Numerics;

namespace Prism.Core.Strings;

internal readonly struct NameEntryHandle
    : IEquatable<NameEntryHandle>,
        IEqualityOperators<NameEntryHandle, NameEntryHandle, bool>
{
    public int Block { get; }
    public int Offset { get; }

    public NameEntryHandle(NameEntryId id)
        : this(
            (int)(id.ToUnstableInt() >> Name.BlockOffsetBits),
            (int)(id.ToUnstableInt() & Name.BlockOffsets - 1)
        ) { }

    public NameEntryHandle(int block, int offset)
    {
        Block = block;
        Offset = offset;
        Debug.Assert(block < Name.MaxBlockBits);
        Debug.Assert(offset < Name.BlockOffsets);
    }

    public static implicit operator NameEntryHandle(NameEntryId id) => new(id);

    public static implicit operator NameEntryId(NameEntryHandle handle)
    {
        return NameEntryId.FromUnstableInt(
            (uint)handle.Block << Name.BlockOffsetBits | (uint)handle.Offset
        );
    }

    public bool Equals(NameEntryHandle other)
    {
        return Block == other.Block && Offset == other.Offset;
    }

    public override bool Equals(object? obj)
    {
        return obj is NameEntryHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (Block << (32 - Name.MaxBlockBits))
            + Block
            + (Offset << (Name.BlockOffsetBits - 1))
            + Offset
            + (Offset >> 4);
    }

    public static bool operator ==(NameEntryHandle left, NameEntryHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NameEntryHandle left, NameEntryHandle right)
    {
        return !(left.Equals(right));
    }
}
