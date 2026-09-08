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
    private readonly ControlFlowGraph _cfg;

    private FunctionBodyAnalysis(ControlFlowGraph cfg)
    {
        _cfg = cfg;
    }

    public static FunctionBodyAnalysis Create(BoundStatement body)
    {
        var cfg = ControlFlowGraphBuilder.Build(body);
        return new FunctionBodyAnalysis(cfg);
    }

    public bool IsAddressTaken(Symbol symbol)
    {
        return _cfg.IsAddressTaken(symbol);
    }
}
