// @file MirFunctionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;
using Prism.Core.Mir;
using Prism.Core.Symbols;

namespace Prism.Core.Mir;

internal sealed class MirFunctionBuilder
{
    private readonly FunctionSymbol _function;

    private readonly List<MirLocal> _locals = [];
    private readonly List<MirBasicBlockBuilder> _blocks = [];

    private int _nextLocalId;
    private int _nextBlockId;
    private MirBlockId? _entryBlock;

    public MirFunctionBuilder(FunctionSymbol function)
    {
        _function = function;
        _locals.EnsureCapacity(
            (int)BitOperations.RoundUpToPowerOf2((uint)function.Parameters.Length)
        );
        foreach (var parameter in function.Parameters)
        {
            _locals.Add(new MirParameter(new MirLocalId(_nextLocalId++), parameter));
        }
    }

    public TypeSymbol ReturnType => _function.ReturnType;

    public MirBlockId EntryBlock =>
        _entryBlock ?? throw new InvalidOperationException("Entry block has not been set.");

    public IReadOnlyList<MirLocal> Locals => _locals;

    public MirLocal AddLocalVariable(VariableSymbol variable)
    {
        var local = new MirLocalVariable(new MirLocalId(_nextLocalId++), variable);
        _locals.Add(local);
        return local;
    }

    public MirLocal AddTemporary(string name, TypeSymbol type)
    {
        var local = new MirTemporary(new MirLocalId(_nextLocalId++), name, type);
        _locals.Add(local);
        return local;
    }

    public MirBasicBlockBuilder AddDetachedBlock(string name)
    {
        return new MirBasicBlockBuilder(new MirBlockId(_nextBlockId++), name);
    }

    public MirBasicBlockBuilder AddBlock(string name)
    {
        var block = new MirBasicBlockBuilder(new MirBlockId(_nextBlockId++), name);
        _blocks.Add(block);
        return block;
    }

    public void AddBlock(MirBasicBlockBuilder block)
    {
        _blocks.Add(block);
    }

    public void SetEntryBlock(MirBlockId blockId)
    {
        if (_blocks.All(b => b.Id != blockId))
            throw new ArgumentOutOfRangeException(
                nameof(blockId),
                "Entry block must already exist."
            );

        _entryBlock = blockId;
    }

    public MirFunction Build()
    {
        if (_entryBlock is null)
            throw new InvalidOperationException("Entry block was not set.");

        var blocks = new MirBasicBlock[_blocks.Count];
        foreach (var (i, block) in _blocks.Index())
        {
            blocks[i] = block.Build();
        }

        return new MirFunction(
            _function,
            [.. _locals],
            ImmutableCollectionsMarshal.AsImmutableArray(blocks),
            _entryBlock.Value
        );
    }
}
