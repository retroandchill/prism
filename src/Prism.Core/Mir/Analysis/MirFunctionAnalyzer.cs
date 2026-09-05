// @file MirFunctionAnalyzer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Utils;

namespace Prism.Core.Mir.Analysis;

internal sealed class MirFunctionAnalyzer
{
    public MirControlFlowGraph AnalyzeControlFlow(MirFunction function)
    {
        var blocks = function.Blocks;
        var entryBlock = function.GetEntryBlock();
        var successors = new Dictionary<MirBlockId, List<MirBlockId>>();
        var predecessors = new Dictionary<MirBlockId, List<MirBlockId>>();
        var explored = new HashSet<MirBlockId>();
        var toExplore = new Queue<MirBlockId>();
        toExplore.Enqueue(entryBlock.Id);

        while (toExplore.TryDequeue(out var blockId))
        {
            var block = function.GetBlock(blockId);
            explored.Add(block.Id);
            var successorList = successors.GetOrAdd(block.Id, () => []);
            switch (block.Terminator)
            {
                case MirGotoTerminator gotoTerminator:
                    AddSuccessor(gotoTerminator.Target, successorList);
                    AddPredecessor(gotoTerminator.Target, block.Id);
                    break;
                case MirBranchTerminator branchTerminator:
                    Debug.Assert(branchTerminator.WhenTrue != branchTerminator.WhenFalse);
                    AddSuccessor(branchTerminator.WhenTrue, successorList);
                    AddPredecessor(branchTerminator.WhenTrue, block.Id);
                    AddSuccessor(branchTerminator.WhenFalse, successorList);
                    AddPredecessor(branchTerminator.WhenFalse, block.Id);
                    break;
            }
        }

        return new MirControlFlowGraph
        {
            Blocks = blocks,
            Successors = successors.ToImmutableDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToImmutableArray()
            ),
            Predecessors = predecessors.ToImmutableDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToImmutableArray()
            ),
        };

        void AddPredecessor(MirBlockId successor, MirBlockId predecessor)
        {
            var predecessorList = predecessors.GetOrAdd(successor, () => []);
            predecessorList.Add(predecessor);
        }

        void AddSuccessor(MirBlockId successor, List<MirBlockId> successorList)
        {
            successorList.Add(successor);
            if (!explored.Contains(successor))
                toExplore.Enqueue(successor);
        }
    }
}
