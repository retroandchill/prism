// @file MirFunctionAnalyzer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Abi;
using Prism.Core.Compiling;
using Prism.Core.Utils;

namespace Prism.Core.Mir.Analysis;

internal static class MirFunctionAnalyzer
{
    public static MirControlFlowGraph AnalyzeControlFlow(
        MirFunction function,
        CancellationToken cancellationToken
    )
    {
        // By using a builder instead of just copying the blocks outright, we can use this to essentially track
        // any blocks that are dead code.
        var blocks = ImmutableArray.CreateBuilder<MirBasicBlock>(function.Blocks.Length);
        var entryBlock = function.EntryBlock;
        var successors = new Dictionary<MirBlockId, List<MirBlockId>>();
        var predecessors = new Dictionary<MirBlockId, List<MirBlockId>>();
        var explored = new HashSet<MirBlockId>();
        var toExplore = new Queue<MirBlockId>();
        toExplore.Enqueue(entryBlock);

        while (toExplore.TryDequeue(out var blockId))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!explored.Add(blockId))
                continue;
            var block = function.GetBlock(blockId);
            blocks.Add(block);
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
            Blocks = blocks.DrainToImmutable(),
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

    public static MirLocalFlowAnalysis AnalyzeLocalFlow(
        MirFunction function,
        MirControlFlowGraph cfg,
        CancellationToken cancellationToken
    )
    {
        var builders = function.Locals.ToDictionary(
            l => l.Id,
            l => new MirLocalFlowInfoBuilder(l) { WriteCount = l is MirParameter ? 1 : 0 }
        );

        foreach (var block in cfg.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AnalyzeInstructionLocalFlow(instruction, block.Id, builders);
            }

            cancellationToken.ThrowIfCancellationRequested();
            AnalyzeTerminatorLocalFlow(block.Terminator, block.Id, builders);
        }

        foreach (var builder in builders.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FinalizeLocalFlow(builder, cfg);
        }

        return new MirLocalFlowAnalysis
        {
            Locals = builders.ToImmutableDictionary(kv => kv.Key, kv => kv.Value.Build()),
        };
    }

    private static void AnalyzeInstructionLocalFlow(
        MirInstruction instruction,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        switch (instruction)
        {
            case MirLoadInstruction load:
                AnalyzeReadFromPlace(load.Source, blockId, builders);
                break;

            case MirStoreInstruction assign:
                AnalyzeWriteDestination(assign.Destination, blockId, builders);
                AnalyzeValue(assign.Source, blockId, builders);
                break;

            case MirUnaryInstruction unary:
                AnalyzeValue(unary.Value, blockId, builders);
                break;

            case MirBinaryInstruction binary:
                AnalyzeValue(binary.Left, blockId, builders);
                AnalyzeValue(binary.Right, blockId, builders);
                break;

            case MirConvertInstruction convert:
                AnalyzeValue(convert.Value, blockId, builders);
                break;

            case MirCallInstruction call:
                foreach (var argument in call.Arguments)
                {
                    AnalyzeValue(argument, blockId, builders);
                }

                break;

            case MirIsNotNullInstruction isNotNull:
                AnalyzeValue(isNotNull.Value, blockId, builders);
                break;

            case MirGetNullablePayloadInstruction getNullablePayload:
                AnalyzeValue(getNullablePayload.Value, blockId, builders);
                break;

            case MirMakeNullableInstruction makeNullable:
                if (makeNullable.Payload is not null)
                {
                    AnalyzeValue(makeNullable.Payload, blockId, builders);
                }

                break;

            case MirPhiInstruction:
            case MirStorageLiveInstruction:
            case MirStorageDeadInstruction:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private static void AnalyzeTerminatorLocalFlow(
        MirTerminator terminator,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        switch (terminator)
        {
            case MirBranchTerminator branch:
                AnalyzeValue(branch.Condition, blockId, builders);
                break;

            case MirReturnTerminator { Value: not null } ret:
                AnalyzeValue(ret.Value, blockId, builders);
                break;

            case MirGotoTerminator:
            case MirReturnTerminator:
            case MirUnreachableTerminator:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(terminator));
        }
    }

    private static void AnalyzeValue(
        MirValue value,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        switch (value)
        {
            case MirConstantValue:
            case MirNullValue:
            case MirVoidValue:
            case MirSsaValue:
                return;

            case MirAddressOfValue addressOf:
                AnalyzeAddressOfPlace(addressOf.Place, builders);
                AnalyzePlaceComputation(addressOf.Place, blockId, builders);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    private static void AnalyzeReadFromPlace(
        MirPlace place,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        switch (place)
        {
            case MirLocalPlace local:
                RecordRead(local.LocalId, blockId, builders);
                return;

            case MirGlobalPlace:
                return;

            case MirDerefPlace deref:
                AnalyzeValue(deref.Pointer, blockId, builders);
                return;

            case MirIndexPlace index:
                AnalyzeReadFromPlace(index.Base, blockId, builders);
                AnalyzeValue(index.Index, blockId, builders);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(place));
        }
    }

    private static void AnalyzePlaceComputation(
        MirPlace place,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        switch (place)
        {
            case MirLocalPlace:
            case MirGlobalPlace:
                return;

            case MirDerefPlace deref:
                AnalyzeValue(deref.Pointer, blockId, builders);
                return;

            case MirIndexPlace index:
                AnalyzePlaceComputation(index.Base, blockId, builders);
                AnalyzeValue(index.Index, blockId, builders);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(place));
        }
    }

    private static void AnalyzeWriteDestination(
        MirPlace place,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        AnalyzePlaceComputation(place, blockId, builders);

        if (place is MirLocalPlace local)
        {
            RecordWrite(local.LocalId, blockId, builders);
        }
        else if (place is MirIndexPlace indexer)
        {
            AnalyzeWriteDestination(indexer.Base, blockId, builders);
        }
    }

    private static void AnalyzeAddressOfPlace(
        MirPlace place,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        switch (place)
        {
            case MirLocalPlace local:
                builders[local.LocalId].IsAddressTaken = true;
                return;

            case MirGlobalPlace:
                return;

            case MirDerefPlace:
                return;

            case MirIndexPlace index:
                AnalyzeAddressOfPlace(index.Base, builders);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(place));
        }
    }

    private static void RecordRead(
        MirLocalId localId,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        var builder = builders[localId];
        builder.ReadCount++;
        builder.UseBlocks.Add(blockId);
    }

    private static void RecordWrite(
        MirLocalId localId,
        MirBlockId blockId,
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders
    )
    {
        var builder = builders[localId];
        builder.WriteCount++;
        builder.DefBlocks.Add(blockId);
    }

    private static void FinalizeLocalFlow(MirLocalFlowInfoBuilder builder, MirControlFlowGraph cfg)
    {
        builder.IsUsedAcrossBlocks = IsUsedAcrossBlocks(builder);
        builder.HasMultipleDefinitions = HasMultipleDefinitions(builder);
    }

    private static bool IsUsedAcrossBlocks(MirLocalFlowInfoBuilder builder)
    {
        if (builder.UseBlocks.Count > 1 || builder.DefBlocks.Count > 1)
            return true;

        if (builder.UseBlocks.Count == 0 || builder.DefBlocks.Count == 0)
            return false;

        return builder.UseBlocks[0] != builder.DefBlocks[0];
    }

    private static bool HasMultipleDefinitions(MirLocalFlowInfoBuilder builder)
    {
        return builder.WriteCount > 1;
    }

    public static MirLocalClassificationAnalysis ClassifyLocals(
        Compilation compilation,
        MirFunction function,
        MirLocalFlowAnalysis localFlow,
        CancellationToken cancellationToken
    )
    {
        var builder = ImmutableDictionary.CreateBuilder<MirLocalId, MirLocalClassification>();

        var abi = compilation.GetFunctionAbi(function.Symbol);
        foreach (var (key, local) in localFlow.Locals)
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.Add(key, ClassifyLocal(abi, local));
        }

        return new MirLocalClassificationAnalysis { Locals = builder.ToImmutable() };
    }

    private static MirLocalClassification ClassifyLocal(FunctionAbi abi, MirLocalFlowInfo local)
    {
        if (
            local.Local is MirParameter { Parameter: var parameter }
            && abi.IsParameterIndirect(parameter)
        )
        {
            return new MirLocalClassification
            {
                LocalId = local.LocalId,
                StorageKind = MirLocalStorageKind.IndirectParam,
            };
        }

        if (RequiresMemoryStorage(local))
        {
            return new MirLocalClassification
            {
                LocalId = local.LocalId,
                StorageKind = MirLocalStorageKind.Memory,
            };
        }

        return new MirLocalClassification
        {
            LocalId = local.LocalId,
            StorageKind = MirLocalStorageKind.Ssa,
        };
    }

    private static bool RequiresMemoryStorage(MirLocalFlowInfo local)
    {
        return local.IsAddressTaken || local.IsWrittenIndirectly || local.HasMultipleDefinitions;
    }
}
