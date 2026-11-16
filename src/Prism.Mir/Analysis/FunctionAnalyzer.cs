using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Utils;

namespace Prism.Mir.Analysis;

internal static class FunctionAnalyzer
{
    public static ControlFlowGraph AnalyzeControlFlow(
        Function function,
        CancellationToken cancellationToken
    )
    {
        var blocks = ImmutableArray.CreateBuilder<BasicBlock>(function.Blocks.Length);
        var successors = new Dictionary<BasicBlock, List<BasicBlock>>();
        var predecessors = new Dictionary<BasicBlock, List<BasicBlock>>();
        var explored = new HashSet<BasicBlock>();
        var toExplore = new Queue<BasicBlock>();
        toExplore.Enqueue(function.EntryBlock);

        while (toExplore.TryDequeue(out var block))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!explored.Add(block))
                continue;

            blocks.Add(block);
            var successorList = successors.GetOrAdd(block, () => []);
            switch (block.Terminator)
            {
                case BranchTerminator branchTerminator:
                    Debug.Assert(branchTerminator.TrueBlock != branchTerminator.FalseBlock);
                    AddSuccessor(branchTerminator.TrueBlock, successorList);
                    AddPredecessor(branchTerminator.TrueBlock, block);
                    AddSuccessor(branchTerminator.FalseBlock, successorList);
                    AddPredecessor(branchTerminator.FalseBlock, block);
                    break;
                case CallTerminator callTerminator:
                    Debug.Assert(callTerminator.NextBlock != callTerminator.UnwindBlock);
                    AddSuccessor(callTerminator.NextBlock, successorList);
                    AddPredecessor(callTerminator.NextBlock, block);
                    if (callTerminator.UnwindBlock is not null)
                    {
                        AddSuccessor(callTerminator.UnwindBlock, successorList);
                        AddPredecessor(callTerminator.UnwindBlock, block);
                    }

                    break;
                case GotoTerminator gotoTerminator:
                    AddSuccessor(gotoTerminator.Target, successorList);
                    AddPredecessor(gotoTerminator.Target, block);
                    break;
            }
        }

        return new ControlFlowGraph(
            blocks.DrainToImmutable(),
            successors.ToFrozenDictionary(t => t.Key, t => t.Value.ToImmutableArray()),
            predecessors.ToFrozenDictionary(t => t.Key, t => t.Value.ToImmutableArray())
        );

        void AddPredecessor(BasicBlock successor, BasicBlock predecessor)
        {
            var predecessorList = predecessors.GetOrAdd(successor, () => []);
            predecessorList.Add(predecessor);
        }

        void AddSuccessor(BasicBlock successor, List<BasicBlock> successorList)
        {
            successorList.Add(successor);
            if (!explored.Contains(successor))
                toExplore.Enqueue(successor);
        }
    }
}
