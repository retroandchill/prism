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
}
