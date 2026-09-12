// @file MirBasicBlock.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Mir;

internal readonly record struct MirBlockId(int Value);

internal sealed class MirBasicBlock
{
    public MirBasicBlock(
        MirBlockId id,
        string name,
        ImmutableArray<MirInstruction> instructions,
        MirTerminator terminator
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(terminator);

        Id = id;
        Name = name;
        Instructions = instructions.IsDefault ? [] : instructions;
        Terminator = terminator;
    }

    public MirBlockId Id { get; }

    public string Name { get; }

    public ImmutableArray<MirInstruction> Instructions { get; }

    public MirTerminator Terminator { get; }
}
