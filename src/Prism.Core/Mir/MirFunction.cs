// @file MirFunction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Utils;

namespace Prism.Core.Mir;

internal sealed class MirFunction
{
    public MirFunction(
        FunctionSymbol symbol,
        MirType returnType,
        ImmutableArray<MirLocal> locals,
        ImmutableArray<MirBasicBlock> blocks,
        MirBlockId entryBlock
    )
    {
        ArgumentNullException.ThrowIfNull(symbol);
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

        Symbol = symbol;
        Name = symbol.ToDisplayString();
        ReturnType = returnType;
        Locals = locals.NullToEmpty();
        Blocks = blocks;
        EntryBlock = entryBlock;
    }

    public FunctionSymbol Symbol { get; }

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
