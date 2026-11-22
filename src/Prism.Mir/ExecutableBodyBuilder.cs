// @file ExecutableBodyBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;
using Prism.Utils;

namespace Prism.Mir;

public sealed class ExecutableBodyBuilder
{
    private readonly List<BasicBlockBuilder> _basicBlocks = [];
    private readonly List<Local> _locals = [];

    private uint _nextBasicBlockId = 1;
    private uint _nextLocalId = 1;

    public Local? ReturnValueLocal { get; private set; }

    public BasicBlockBuilder EntryBlock
    {
        get => field ?? throw new InvalidOperationException("Entry block not set");
        set;
    } = null!;

    public ReadOnlyList<Local> Locals => _locals.AsReadOnlyList();

    public ReadOnlyList<BasicBlockBuilder> Blocks => _basicBlocks.AsReadOnlyList();

    public void SetReturnValueLocal(TypeSymbol type)
    {
        ReturnValueLocal = new Local(LocalId.ReturnSlot, type);
        _locals.Add(ReturnValueLocal);
    }

    public BasicBlockBuilder AddBasicBlock(string name = "block")
    {
        var builder = new BasicBlockBuilder(new BasicBlockId(_nextBasicBlockId++), name);
        _basicBlocks.Add(builder);
        return builder;
    }

    public void AddExistingBlock(BasicBlockBuilder builder)
    {
        _basicBlocks.Add(builder);
    }

    public BasicBlockBuilder AddDetachedBlock(string name = "block")
    {
        return new BasicBlockBuilder(new BasicBlockId(_nextBasicBlockId++), name);
    }

    public Local AddLocal(ValueSymbol valueSymbol)
    {
        var local = new Local(new LocalId(_nextLocalId++), valueSymbol);
        _locals.Add(local);
        return local;
    }

    public Local AddLocal(TypeSymbol type)
    {
        var local = new Local(new LocalId(_nextLocalId++), type);
        _locals.Add(local);
        return local;
    }
}
