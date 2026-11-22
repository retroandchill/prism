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

    public ExecutableBodyBuilder ExecutableBody { get; } = new();

    public BasicBlockBuilder EntryBlock
    {
        get => ExecutableBody.EntryBlock;
        set => ExecutableBody.EntryBlock = value;
    }

    public Local? ReturnValueLocal => ExecutableBody.ReturnValueLocal;

    public Local? ReceiverLocal { get; }

    public ImmutableArray<(ParameterSymbol Symbol, Local Local)> Parameters { get; }

    public FunctionBuilder(FunctionSymbol symbol)
    {
        Symbol = symbol;

        if (!Symbol.ReturnsVoid)
        {
            ExecutableBody.SetReturnValueLocal(Symbol.ReturnType);
        }

        if (Symbol.ReceiverType is { } receiver)
        {
            ReceiverLocal = ExecutableBody.AddLocal(receiver);
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
            parameters[i] = (parameter, ExecutableBody.AddLocal(parameter));
        }

        Parameters = ImmutableCollectionsMarshal.AsImmutableArray(parameters);
    }

    public Function Build()
    {
        var function = Function.Mapper.Map(this);
        function.Validate();
        return function.PruneUnusedBlocksAndLocals();
    }
}
