// @file MirLocalClassification.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir.Analysis;

internal sealed class MirLocalClassification
{
    public required MirLocalId LocalId { get; init; }
    public required bool IsSsaEligible { get; init; }
    public required MirLocalStorageKind StorageKind { get; init; }
}
