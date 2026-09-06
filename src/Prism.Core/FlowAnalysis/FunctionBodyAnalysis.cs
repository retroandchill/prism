// @file FunctionBodyAnalysis.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class FunctionBodyAnalysis
{
    private readonly ControlFlowGraph _controlFlowGraph;

    private FunctionBodyAnalysis(ControlFlowGraph controlFlowGraph)
    {
        _controlFlowGraph = controlFlowGraph;
    }

    public static FunctionBodyAnalysis Create(Compilation compilation, FunctionSymbol function)
    {
        var body = compilation.GetBoundBody(function);
        return body is not null
            ? new FunctionBodyAnalysis(ControlFlowGraphBuilder.Build(body))
            : throw new ArgumentException("Function does not have a body", nameof(function));
    }

    public bool IsAddressTaken(Symbol symbol)
    {
        return _controlFlowGraph.IsAddressTaken(symbol);
    }
}
