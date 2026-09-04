// @file MirFunctionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Prism.Core.Mir;

namespace Prism.Core.Mir;

internal sealed class MirFunctionBuilder
{
    private readonly MirFunctionId _functionId;
    private readonly string _name;
    private readonly MirType _returnType;

    private readonly List<MirLocal> _locals = [];
    private readonly List<MirBasicBlockBuilder> _blocks = [];

    private int _nextLocalId;
    private int _nextBlockId;
    private MirBlockId? _entryBlock;

    public MirFunctionBuilder(MirFunctionId functionId, string name, MirType returnType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(returnType);

        _functionId = functionId;
        _name = name;
        _returnType = returnType;
    }

    public string Name => _name;

    public MirType ReturnType => _returnType;

    public MirBlockId EntryBlock =>
        _entryBlock ?? throw new InvalidOperationException("Entry block has not been set.");

    public MirLocal AddLocal(string name, MirType type, MirLocalKind kind, bool isMutable = false)
    {
        var local = new MirLocal(new MirLocalId(_nextLocalId++), name, type, kind, isMutable);

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
            _functionId,
            _name,
            _returnType,
            [.. _locals],
            ImmutableCollectionsMarshal.AsImmutableArray(blocks),
            _entryBlock.Value
        );
    }
}
