// @file Module.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols;

namespace Prism.Mir;

public sealed class Module
{
    public ImmutableArray<VariableSymbol> GlobalVariables { get; }

    public ImmutableArray<Function> Functions { get; }

    public FunctionSymbol? EntryPoint { get; }

    public Module(
        ImmutableArray<VariableSymbol> globalVariables,
        ImmutableArray<Function> functions,
        FunctionSymbol? entryPoint
    )
    {
        GlobalVariables = globalVariables;
        Functions = functions;
        EntryPoint = entryPoint;
    }

    internal Module WithFunctions(ImmutableArray<Function> functions)
    {
        return functions != Functions ? new Module(GlobalVariables, functions, EntryPoint) : this;
    }
}
