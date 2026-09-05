// @file MirPerBlockUseDefAnalysis.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Mir.Analysis;

internal sealed class MirUseDefAnalysis
{
    public required ImmutableDictionary<MirBlockId, MirBlockUseDef> Blocks { get; init; }
}
