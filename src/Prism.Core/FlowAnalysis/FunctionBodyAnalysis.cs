// @file FunctionBodyAnalysis.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class FunctionBodyAnalysis
{
    private readonly BoundStatement _body;

    private FunctionBodyAnalysis(BoundStatement body)
    {
        _body = body;
    }

    public static FunctionBodyAnalysis Create(Compilation compilation, FunctionSymbol function)
    {
        var body = compilation.GetBoundBody(function);
        return body is not null
            ? new FunctionBodyAnalysis(body)
            : throw new ArgumentException("Function does not have a body", nameof(function));
    }

    private ControlFlowGraph ControlFlowGraph
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(ref field, ControlFlowGraphBuilder.Build(_body), null);
            return field;
        }
    }

    public bool IsAddressTaken(Symbol symbol)
    {
        return ControlFlowGraph.IsAddressTaken(symbol);
    }
}
