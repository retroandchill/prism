// @file MirBasicBlockBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Mir;

internal sealed class MirBasicBlockBuilder
{
    private readonly List<MirInstruction> _instructions = [];
    private MirTerminator? _terminator;

    public MirBasicBlockBuilder(MirBlockId id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public MirBlockId Id { get; }

    public string Name { get; }

    public bool IsTerminated => _terminator is not null;

    public void AddInstruction(MirInstruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        if (_terminator is not null)
            throw new InvalidOperationException("Cannot add instructions after a terminator.");

        _instructions.Add(instruction);
    }

    public void SetTerminator(MirTerminator terminator)
    {
        ArgumentNullException.ThrowIfNull(terminator);

        if (_terminator is not null)
            throw new InvalidOperationException("A basic block can only have one terminator.");

        _terminator = terminator;
    }

    public MirBasicBlock Build()
    {
        return new MirBasicBlock(
            Id,
            Name,
            [.. _instructions],
            _terminator ?? throw new InvalidOperationException("Block is missing a terminator.")
        );
    }
}
