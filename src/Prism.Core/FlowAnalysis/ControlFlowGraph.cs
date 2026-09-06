// @file ControlFlowGraph.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class ControlFlowGraph(
    ControlFlowBasicBlock entry,
    ControlFlowBasicBlock exit,
    ImmutableArray<ControlFlowBasicBlock> blocks,
    ImmutableHashSet<Symbol> addressedLocals
)
{
    public ControlFlowBasicBlock Entry { get; } = entry;

    public ControlFlowBasicBlock Exit { get; } = exit;

    public ImmutableArray<ControlFlowBasicBlock> Blocks { get; } = blocks;

    public ImmutableHashSet<Symbol> AddressedLocals { get; } = addressedLocals;

    public bool IsAddressTaken(Symbol symbol)
    {
        return AddressedLocals.Contains(symbol);
    }
}
