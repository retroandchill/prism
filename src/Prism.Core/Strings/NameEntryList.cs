// @file NameEntryList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Prism.Core.Utils;

namespace Prism.Core.Strings;

[InlineArray((int)Names.MaxBlocks)]
internal struct NameEntryListBlocks
{
    private string[]? _element;
}

internal sealed class NameEntryList
{
    public const int BlockSize = 1024;
    private readonly ReaderWriterLockSlim _lock = new();
    private uint _currentBlock = 0;
    private uint _currentBlockCursor = 0;
    private NameEntryListBlocks _blocks;

    public uint NumBlocks => _currentBlock + 1;

    public NameEntryList()
    {
        _blocks[0] = AllocateBlock();
    }

    public void ReserveBlocks(uint numBlocks)
    {
        using var scope = _lock.EnterWriteScope();
        for (var i = numBlocks - 1; i > _currentBlock && _blocks[(int)i] is null; i--)
        {
            _blocks[(int)i] = AllocateBlock();
        }
    }

    public NameEntryHandle GetBlockSlot()
    {
        using var scope = _lock.EnterWriteScope();

        if (_currentBlockCursor == _blocks[(int)_currentBlock]!.Length)
        {
            AllocateNewBlock();
        }

        return new NameEntryHandle(_currentBlock, _currentBlockCursor++);
    }

    public NameEntryHandle Create(string name)
    {
        var handle = GetBlockSlot();
        ref var entry = ref Resolve(handle);
        entry = name;
        return handle;
    }

    public ref string Resolve(NameEntryHandle handle)
    {
        return ref _blocks[(int)handle.Block]![handle.Offset];
    }

    private static string[] AllocateBlock()
    {
        return new string[BlockSize];
    }

    private void AllocateNewBlock()
    {
        ++_currentBlock;
        _currentBlockCursor = 0;

        _blocks[(int)_currentBlock] ??= AllocateBlock();
    }
}
