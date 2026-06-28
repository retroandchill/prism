// @file Names.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Strings;

internal static class Names
{
    public const uint MaxBlockBits = 13;
    public const uint BlockOffsetBits = 16;
    public const uint MaxBlocks = 1 << (int)MaxBlockBits;
    public const uint BlockOffsets = 1 << (int)BlockOffsetBits;

    private const uint PoolShardBits = 10;
    public const uint PoolShards = 1 << (int)PoolShardBits;
    private const uint PoolInitialSlotBits = 8;
    public const int PoolInitialSLotsPerShard = 1 << (int)PoolInitialSlotBits;
}
