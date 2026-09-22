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

internal sealed record MirLoadInstruction(MirSsaValue Result, MirPlace Source) : MirInstruction;

internal sealed record MirStoreInstruction(MirPlace Destination, MirValue Source) : MirInstruction;

internal readonly record struct PhiSource(MirValue Value, MirBlockId Block);

internal sealed record MirPhiInstruction(MirSsaValue Result, ImmutableArray<PhiSource> Sources)
    : MirInstruction;

internal sealed record MirUnaryInstruction(MirSsaValue Result, MirUnaryOp Op, MirValue Value)
    : MirInstruction;

internal sealed record MirBinaryInstruction(
    MirSsaValue Result,
    MirBinaryOp Op,
    MirValue Left,
    MirValue Right
) : MirInstruction;

internal sealed record MirConvertInstruction(
    MirSsaValue Result,
    Conversion Conversion,
    MirValue Value
) : MirInstruction;

internal sealed record MirCallInstruction(
    MirSsaValue? Result,
    FunctionSymbol Callee,
    ImmutableArray<MirValue> Arguments
) : MirInstruction;

internal sealed record MirIsNotNullInstruction(MirSsaValue Result, MirValue Value) : MirInstruction;

internal sealed record MirGetNullablePayloadInstruction(MirSsaValue Result, MirValue Value)
    : MirInstruction;

internal sealed record MirMakeNullableInstruction(MirSsaValue Result, MirValue? Payload)
    : MirInstruction;

internal sealed record MirStorageLiveInstruction(MirLocalId LocalId) : MirInstruction;

internal sealed record MirStorageDeadInstruction(MirLocalId LocalId) : MirInstruction;
