// @file FunctionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;

namespace Prism.Mir;

public sealed class FunctionBuilder
{
    public FunctionSymbol Symbol { get; }

    private readonly List<BasicBlockBuilder> _basicBlocks = [];
    private readonly List<Local> _locals = [];

    private uint _nextBasicBlockId = 1;
    private uint _nextLocalId = 1;

    public FunctionBuilder(FunctionSymbol symbol)
    {
        Symbol = symbol;

        if (!Symbol.ReturnsVoid)
        {
            _locals.Add(new Local(LocalId.ReturnSlot, Symbol.ReturnType));
        }

        if (Symbol.ReceiverType is { } receiver)
        {
            AddLocal(receiver);
        }

        foreach (var parameter in Symbol.Parameters)
        {
            AddLocal(parameter.Type);
        }
    }

    internal IReadOnlyList<Local> Locals => _locals;

    internal IReadOnlyList<BasicBlockBuilder> Blocks => _basicBlocks;

    public BasicBlockBuilder AddBasicBlock(string name = "block")
    {
        var builder = new BasicBlockBuilder(new BasicBlockId(_nextBasicBlockId), name);
        _basicBlocks.Add(builder);
        _nextBasicBlockId++;
        return builder;
    }

    public Local AddLocal(TypeSymbol type)
    {
        var local = new Local(new LocalId(_nextLocalId), type);
        _locals.Add(local);
        _nextLocalId++;
        return local;
    }

    public Function Build()
    {
        return Function.Mapper.Map(this);
    }
}
