// @file ControlFlowEdge.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.BoundTree;

namespace Prism.Core.FlowAnalysis;

internal enum ControlFlowBranchKind : byte
{
    FallThrough,
    ConditionalTrue,
    ConditionalFalse,
    Jump,
    Return,
}

internal sealed class ControlFlowEdge(
    ControlFlowBasicBlock from,
    ControlFlowBasicBlock to,
    ControlFlowBranchKind kind,
    BoundExpression? condition = null
)
{
    public ControlFlowBasicBlock From { get; } = from;

    public ControlFlowBasicBlock To { get; } = to;

    public ControlFlowBranchKind Kind { get; } = kind;

    public BoundExpression? Condition { get; } = condition;
}
