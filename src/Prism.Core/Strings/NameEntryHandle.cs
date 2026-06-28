// @file NameEntryHandle.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace Prism.Core.Strings;

internal readonly record struct NameEntryHandle(uint Block, uint Offset)
    : IEqualityOperators<NameEntryHandle, NameEntryHandle, bool>
{
    public NameEntryHandle(NameEntryId id)
        : this(
            id.ToUnstableInt() >> (int)Names.BlockOffsetBits,
            id.ToUnstableInt() & (Names.BlockOffsets - 1)
        ) { }

    public static implicit operator NameEntryHandle(NameEntryId id) => new(id);

    public static implicit operator NameEntryId(NameEntryHandle handle)
    {
        return NameEntryId.FromUnstableInt(
            handle.Block << (int)Names.BlockOffsetBits | handle.Offset
        );
    }

    public override int GetHashCode()
    {
        return (int)(
            (Block << (int)(32 - Names.MaxBlockBits))
            + Block
            + (Offset << (int)Names.BlockOffsetBits)
            + Offset
            + (Offset >> 4)
        );
    }
}
