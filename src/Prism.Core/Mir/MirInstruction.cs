// @file MirInstruction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Utils;

namespace Prism.Core.Mir;

internal abstract record MirInstruction;

internal sealed record MirAssignInstruction(MirPlace Destination, MirValue Source) : MirInstruction;

internal sealed record MirEvalInstruction(MirValue Value) : MirInstruction;

internal sealed record MirCallInstruction(
    MirPlace? Destination,
    MirFunctionId Callee,
    ImmutableArray<MirValue> Arguments
) : MirInstruction;

internal sealed record MirStorageLiveInstruction(MirLocalId LocalId) : MirInstruction;

internal sealed record MirStorageDeadInstruction(MirLocalId LocalId) : MirInstruction;
