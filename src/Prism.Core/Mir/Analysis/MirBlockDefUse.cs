// @file MirBlockDefUse.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Mir.Analysis;

internal sealed class MirPerBlockUseDefAnalysis
{
    public required ImmutableDictionary<MirBlockId, MirBlockUseDef> Blocks { get; init; }
}

internal sealed class MirBlockUseDef
{
    public required MirBlockId BlockId { get; init; }
    public required ImmutableHashSet<MirLocalId> Uses { get; init; }
    public required ImmutableHashSet<MirLocalId> Defs { get; init; }
    public required ImmutableHashSet<MirLocalId> AllReads { get; init; }
    public required ImmutableHashSet<MirLocalId> AllWrites { get; init; }
}
