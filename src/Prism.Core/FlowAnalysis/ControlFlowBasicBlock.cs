// @file ControlFlowBasicBlock.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.BoundTree;

namespace Prism.Core.FlowAnalysis;

internal sealed class ControlFlowBasicBlock(int ordinal, bool isEntry = false, bool isExit = false)
{
    public int Ordinal { get; } = ordinal;

    public bool IsEntry { get; } = isEntry;

    public bool IsExit { get; } = isExit;

    public List<BoundStatement> Statements { get; } = [];

    public List<ControlFlowEdge> Incoming { get; } = [];

    public List<ControlFlowEdge> Outgoing { get; } = [];

    public override string ToString()
    {
        return IsEntry ? $"Block B{Ordinal} [entry]"
            : IsExit ? $"Block B{Ordinal} [exit]"
            : $"Block B{Ordinal}";
    }
}
