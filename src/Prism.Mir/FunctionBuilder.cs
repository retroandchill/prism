// @file FunctionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Semantic.Symbols;
using Prism.Utils;
using ZLinq;

namespace Prism.Mir;

public sealed class FunctionBuilder
{
    public FunctionSymbol Symbol { get; }

    internal ExecutableBodyBuilder ExecutableBody { get; } = new();

    public BasicBlockBuilder EntryBlock
    {
        get => ExecutableBody.EntryBlock;
        set => ExecutableBody.EntryBlock = value;
    }

    public Local? ReturnValueLocal { get; }

    public Local? ReceiverLocal { get; }

    public ImmutableArray<(ParameterSymbol Symbol, Local Local)> Parameters { get; }

    public FunctionBuilder(FunctionSymbol symbol)
    {
        Symbol = symbol;

        if (!Symbol.ReturnsVoid)
        {
            ReturnValueLocal = new Local(LocalId.ReturnSlot, Symbol.ReturnType);
            ExecutableBody.AddLocal(ReturnValueLocal);
        }

        if (Symbol.ReceiverType is { } receiver)
        {
            ReceiverLocal = AddLocal(receiver);
            Debug.Assert(ReceiverLocal.Id == LocalId.ThisSlot);
        }

        if (Symbol.Parameters.IsEmpty)
        {
            Parameters = [];
            return;
        }

        var parameters = new (ParameterSymbol, Local)[Symbol.Parameters.Length];
        foreach (var (i, parameter) in Symbol.Parameters.AsValueEnumerable().Index())
        {
            parameters[i] = (parameter, AddLocal(parameter));
        }

        Parameters = ImmutableCollectionsMarshal.AsImmutableArray(parameters);
    }

    public ReadOnlyList<Local> Locals => ExecutableBody.Locals;

    public ReadOnlyList<BasicBlockBuilder> Blocks => ExecutableBody.Blocks;

    public BasicBlockBuilder AddBasicBlock(string name = "block")
    {
        return ExecutableBody.AddBasicBlock(name);
    }

    public void AddExistingBlock(BasicBlockBuilder builder)
    {
        ExecutableBody.AddExistingBlock(builder);
    }

    public BasicBlockBuilder AddDetachedBlock(string name = "block")
    {
        return ExecutableBody.AddDetachedBlock(name);
    }

    public Local AddLocal(ValueSymbol valueSymbol)
    {
        return ExecutableBody.AddLocal(valueSymbol);
    }

    public Local AddLocal(TypeSymbol type)
    {
        return ExecutableBody.AddLocal(type);
    }

    public Function Build()
    {
        var function = Function.Mapper.Map(this);
        function.Validate();
        return function.PruneUnusedBlocksAndLocals();
    }
}
