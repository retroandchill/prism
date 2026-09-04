// @file MirFunction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Mir;
using Prism.Core.Utils;

namespace Prism.Core.Mir;

internal readonly record struct MirFunctionId(int Value);

internal sealed class MirFunction
{
    public MirFunction(
        MirFunctionId id,
        string name,
        MirType returnType,
        ImmutableArray<MirLocal> locals,
        ImmutableArray<MirBasicBlock> blocks,
        MirBlockId entryBlock
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(returnType);

        if (blocks.IsDefaultOrEmpty)
            throw new ArgumentException(
                "A MIR function must contain at least one block.",
                nameof(blocks)
            );

        if (!ContainsBlock(blocks, entryBlock))
            throw new ArgumentOutOfRangeException(
                nameof(entryBlock),
                "Entry block must exist in the function."
            );

        Id = id;
        Name = name;
        ReturnType = returnType;
        Locals = locals.NullToEmpty();
        Blocks = blocks;
        EntryBlock = entryBlock;
    }

    public MirFunctionId Id { get; }

    public string Name { get; }

    public MirType ReturnType { get; }

    public ImmutableArray<MirLocal> Locals { get; }

    public ImmutableArray<MirBasicBlock> Blocks { get; }

    public MirBlockId EntryBlock { get; }

    public MirBasicBlock GetEntryBlock() => GetBlock(EntryBlock);

    public MirBasicBlock GetBlock(MirBlockId id)
    {
        return Blocks.FirstOrDefault(block => block.Id == id)
            ?? throw new KeyNotFoundException($"No MIR block exists with id '{id.Value}'.");
    }

    public MirLocal GetLocal(MirLocalId id)
    {
        return Locals.FirstOrDefault(local => local.Id == id)
            ?? throw new KeyNotFoundException($"No MIR local exists with id '{id.Value}'.");
    }

    private static bool ContainsBlock(ImmutableArray<MirBasicBlock> blocks, MirBlockId id)
    {
        return blocks.Any(block => block.Id == id);
    }
}
