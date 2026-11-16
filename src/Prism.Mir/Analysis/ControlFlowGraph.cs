using System.Collections.Frozen;
using System.Collections.Immutable;
using Riok.Mapperly.Abstractions;

namespace Prism.Mir.Analysis;

using GraphEdges = FrozenDictionary<BasicBlock, ImmutableArray<BasicBlock>>;

public sealed class ControlFlowGraph
{
    public ImmutableArray<BasicBlock> Nodes { get; }

    public GraphEdges Successors { get; }

    public GraphEdges Predecessors { get; }

    internal ControlFlowGraph(
        ImmutableArray<BasicBlock> nodes,
        GraphEdges successors,
        GraphEdges predecessors
    )
    {
        Nodes = nodes;
        Successors = successors;
        Predecessors = predecessors;
    }
}
