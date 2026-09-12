// @file MirControlFlowGraph.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Utils;

namespace Prism.Core.Mir.Analysis;

internal sealed class MirControlFlowGraph
{
    public required ImmutableArray<MirBasicBlock> Blocks { get; init; }
    public required ImmutableDictionary<
        MirBlockId,
        ImmutableArray<MirBlockId>
    > Successors { get; init; }
    public required ImmutableDictionary<
        MirBlockId,
        ImmutableArray<MirBlockId>
    > Predecessors { get; init; }
}
