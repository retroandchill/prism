// @file FunctionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Semantic.Symbols;
using Prism.Utils;
using ZLinq;

namespace Prism.Mir;

public sealed class FunctionBuilder
{
    public FunctionSymbol Symbol { get; }

    private readonly List<BasicBlockBuilder> _basicBlocks = [];
    private readonly List<Local> _locals = [];

    private uint _nextBasicBlockId = 1;
    private uint _nextLocalId = 1;

    public BasicBlockBuilder EntryBlock
    {
        get => field ?? throw new InvalidOperationException("Entry block not set");
        set;
    } = null!;

    public Local? ReturnValueLocal { get; }

    public Local? ReceiverLocal { get; }

    public ImmutableArray<(ParameterSymbol Symbol, Local Local)> Parameters { get; }

    public FunctionBuilder(FunctionSymbol symbol)
    {
        Symbol = symbol;

        if (!Symbol.ReturnsVoid)
        {
            ReturnValueLocal = new Local(LocalId.ReturnSlot, Symbol.ReturnType);
            _locals.Add(ReturnValueLocal);
        }

        if (Symbol.ReceiverType is { } receiver)
        {
            ReceiverLocal = AddLocal(receiver);
        }

        if (Symbol.Parameters.IsEmpty)
        {
            Parameters = [];
            return;
        }

        var parameters = new (ParameterSymbol, Local)[Symbol.Parameters.Length];
        foreach (var (i, parameter) in Symbol.Parameters.AsValueEnumerable().Index())
        {
            parameters[i] = (parameter, AddLocal(parameter.Type));
        }

        Parameters = ImmutableCollectionsMarshal.AsImmutableArray(parameters);
    }

    public ReadOnlyList<Local> Locals => _locals.AsReadOnlyList();

    public ReadOnlyList<BasicBlockBuilder> Blocks => _basicBlocks.AsReadOnlyList();

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

    public Local AddLocal(TypeSymbol type)
    {
        var local = new Local(new LocalId(_nextLocalId++), type);
        _locals.Add(local);
        return local;
    }

    public Function Build()
    {
        var function = Function.Mapper.Map(this);
        function.Validate();
        return function;
    }
}
