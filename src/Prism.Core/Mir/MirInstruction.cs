// @file MirInstruction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal closed record MirInstruction
{
    public SourceLocation? Location { get; init; }
}

internal sealed record MirAssignInstruction(MirPlace Destination, MirValue Source) : MirInstruction;

internal sealed record MirUnaryInstruction(MirPlace Destination, MirUnaryOp Op, MirValue Value)
    : MirInstruction;

internal sealed record MirBinaryInstruction(
    MirPlace Destination,
    MirBinaryOp Op,
    MirValue Left,
    MirValue Right
) : MirInstruction;

internal sealed record MirConvertInstruction(
    MirPlace Destination,
    Conversion Conversion,
    MirValue Value
) : MirInstruction;

internal sealed record MirCallInstruction(
    MirLocalPlace? Destination,
    FunctionSymbol Callee,
    ImmutableArray<MirValue> Arguments
) : MirInstruction;

internal sealed record MirIsNotNullInstruction(MirPlace Destination, MirValue Value)
    : MirInstruction;

internal sealed record MirGetNullablePayloadInstruction(MirPlace Destination, MirValue Value)
    : MirInstruction;

internal sealed record MirMakeNullableInstruction(MirPlace Destination, MirValue? Payload)
    : MirInstruction;

internal sealed record MirStorageLiveInstruction(MirLocalId LocalId) : MirInstruction;

internal sealed record MirStorageDeadInstruction(MirLocalId LocalId) : MirInstruction;
