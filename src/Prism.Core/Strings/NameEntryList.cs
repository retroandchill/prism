// @file NameEntryList.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Core.Utils;

namespace Prism.Core.Strings;

internal sealed class NameEntryList
{
    private readonly ReaderWriterLockSlim _lock = new();
    private int _currentBlock;
    private int _nextIndex;
    private readonly string[]?[] _blocks = new string[Name.MaxBlocks][];

    public int NumBlocks => _currentBlock + 1;

    public NameEntryList()
    {
        _blocks[0] = AllocBlock();
    }

    public void ReserveBlocks(int count)
    {
        using var scope = _lock.EnterWriteScope();

        for (var i = count - 1; i > _currentBlock && _blocks[i] is null; i--)
        {
            _blocks[i] = AllocBlock();
        }
    }

    public NameEntryHandle Create(ReadOnlySpan<char> name)
    {
        using var scope = _lock.EnterWriteScope();

        if (_nextIndex >= Name.BlockOffsets)
        {
            AllocateNewBlock();
        }

        _blocks[_currentBlock]![_nextIndex++] = name.ToString();
        return new NameEntryHandle(_currentBlock, _nextIndex - 1);
    }

    public string Resolve(NameEntryHandle handle)
    {
        return _blocks[handle.Block]![handle.Offset];
    }

    private static string[] AllocBlock()
    {
        return new string[Name.BlockOffsets];
    }

    private void AllocateNewBlock()
    {
        ++_currentBlock;
        _nextIndex = 0;
        Debug.Assert(_currentBlock < Name.MaxBlocks);
        _blocks[_currentBlock] ??= AllocBlock();
    }
}
