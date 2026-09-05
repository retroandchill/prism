// @file MirLocalFlowInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Mir.Analysis;

internal sealed class MirLocalFlowInfo
{
    public required MirLocalId LocalId { get; init; }

    public required int WriteCount { get; init; }
    public required int ReadCount { get; init; }

    public required bool IsAddressTaken { get; init; }
    public required bool IsReadIndirectly { get; init; }
    public required bool IsWrittenIndirectly { get; init; }

    public required ImmutableArray<MirBlockId> DefBlocks { get; init; }
    public required ImmutableArray<MirBlockId> UseBlocks { get; init; }

    public required bool IsUsedAcrossBlocks { get; init; }
    public required bool HasMultipleDefinitions { get; init; }
    public required bool HasMergePotential { get; init; }
}

internal sealed class MirLocalFlowInfoBuilder(MirLocalId localId)
{
    public MirLocalId LocalId { get; } = localId;

    public int ReadCount { get; set; }
    public int WriteCount { get; set; }

    public bool IsAddressTaken { get; set; }
    public bool IsReadIndirectly { get; set; }
    public bool IsWrittenIndirectly { get; set; }

    public List<MirBlockId> DefBlocks { get; } = [];
    public List<MirBlockId> UseBlocks { get; } = [];

    public bool IsUsedAcrossBlocks { get; set; }
    public bool HasMultipleDefinitions { get; set; }
    public bool HasMergePotential { get; set; }

    public MirLocalFlowInfo Build()
    {
        return new MirLocalFlowInfo
        {
            LocalId = LocalId,
            ReadCount = ReadCount,
            WriteCount = WriteCount,
            IsAddressTaken = IsAddressTaken,
            IsReadIndirectly = IsReadIndirectly,
            IsWrittenIndirectly = IsWrittenIndirectly,
            DefBlocks = [.. DefBlocks],
            UseBlocks = [.. UseBlocks],
            IsUsedAcrossBlocks = IsUsedAcrossBlocks,
            HasMultipleDefinitions = HasMultipleDefinitions,
            HasMergePotential = HasMergePotential,
        };
    }
}
