// @file MirFunctionAnalyzer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Mir.Analysis;

internal static class MirFunctionAnalyzer
{
    public static MirControlFlowGraph AnalyzeControlFlow(MirFunction function)
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
            var block = function.GetBlock(blockId);
            explored.Add(block.Id);
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

    public static MirUseDefAnalysis AnalyzeDefUse(MirControlFlowGraph cfg)
    {
        var builder = ImmutableDictionary.CreateBuilder<MirBlockId, MirBlockUseDef>();
        foreach (var block in cfg.Blocks)
        {
            builder.Add(block.Id, AnalyzeBlockUseDef(block));
        }

        return new MirUseDefAnalysis { Blocks = builder.ToImmutable() };
    }

    private static MirBlockUseDef AnalyzeBlockUseDef(MirBasicBlock block)
    {
        var defs = ImmutableHashSet.CreateBuilder<MirLocalId>();
        var uses = ImmutableHashSet.CreateBuilder<MirLocalId>();
        var allWrites = ImmutableHashSet.CreateBuilder<MirLocalId>();
        var allReads = ImmutableHashSet.CreateBuilder<MirLocalId>();
        var definedSoFar = new HashSet<MirLocalId>();
        foreach (var instruction in block.Instructions)
        {
            RecordInstruction(instruction, uses, defs, allReads, allWrites, definedSoFar);
        }

        RecordTerminator(block.Terminator, uses, allReads, definedSoFar);

        return new MirBlockUseDef
        {
            BlockId = block.Id,
            Uses = uses.ToImmutable(),
            Defs = defs.ToImmutable(),
            AllReads = allReads.ToImmutable(),
            AllWrites = allWrites.ToImmutable(),
        };
    }

    private static void RecordInstruction(
        MirInstruction instruction,
        ImmutableHashSet<MirLocalId>.Builder uses,
        ImmutableHashSet<MirLocalId>.Builder defs,
        ImmutableHashSet<MirLocalId>.Builder allReads,
        ImmutableHashSet<MirLocalId>.Builder allWrites,
        HashSet<MirLocalId> definedSoFar
    )
    {
        var reads = new HashSet<MirLocalId>();
        var writes = new HashSet<MirLocalId>();

        CollectInstructionReads(instruction, reads);
        CollectInstructionWrites(instruction, writes);

        foreach (var read in reads)
        {
            allReads.Add(read);
            if (!definedSoFar.Contains(read))
                uses.Add(read);
        }

        foreach (var write in writes)
        {
            allWrites.Add(write);
            defs.Add(write);
            definedSoFar.Add(write);
        }
    }

    private static void CollectInstructionReads(
        MirInstruction instruction,
        HashSet<MirLocalId> reads
    )
    {
        switch (instruction)
        {
            case MirAssignInstruction assign:
                CollectPlaceComputationReads(assign.Destination, reads);
                CollectValueReads(assign.Source, reads);
                break;
            case MirBinaryInstruction binary:
                CollectPlaceComputationReads(binary.Destination, reads);
                CollectValueReads(binary.Left, reads);
                CollectValueReads(binary.Right, reads);
                break;
            case MirCallInstruction call:
                if (call.Destination is not null)
                    CollectPlaceComputationReads(call.Destination, reads);

                foreach (var arg in call.Arguments)
                {
                    CollectValueReads(arg, reads);
                }
                break;
            case MirConvertInstruction conversion:
                CollectPlaceComputationReads(conversion.Destination, reads);
                CollectValueReads(conversion.Value, reads);
                break;
            case MirUnaryInstruction mirUnaryInstruction:
                CollectPlaceComputationReads(mirUnaryInstruction.Destination, reads);
                CollectValueReads(mirUnaryInstruction.Value, reads);
                break;

            case MirStorageLiveInstruction:
            case MirStorageDeadInstruction:
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private static void CollectValueReads(MirValue value, HashSet<MirLocalId> reads)
    {
        switch (value)
        {
            case MirAddressOfValue address:
                CollectReadFromPlace(address.Place, reads);
                break;
            case MirReadValue read:
                CollectReadFromPlace(read.Place, reads);
                break;
        }
    }

    private static void CollectReadFromPlace(MirPlace place, HashSet<MirLocalId> reads)
    {
        switch (place)
        {
            case MirLocalPlace local:
                reads.Add(local.LocalId);
                return;

            case MirGlobalPlace:
                return;

            case MirDerefPlace deref:
                CollectValueReads(deref.Pointer, reads);
                return;

            case MirFieldPlace field:
                CollectReadFromPlace(field.Base, reads);
                return;

            case MirIndexPlace index:
                CollectReadFromPlace(index.Base, reads);
                CollectValueReads(index.Index, reads);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(place));
        }
    }

    private static void CollectPlaceComputationReads(MirPlace place, HashSet<MirLocalId> reads)
    {
        switch (place)
        {
            case MirLocalPlace:
            case MirGlobalPlace:
                return;

            case MirDerefPlace deref:
                CollectValueReads(deref.Pointer, reads);
                return;

            case MirFieldPlace field:
                CollectPlaceComputationReads(field.Base, reads);
                return;

            case MirIndexPlace index:
                CollectPlaceComputationReads(index.Base, reads);
                CollectValueReads(index.Index, reads);
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(place));
        }
    }

    private static void CollectInstructionWrites(
        MirInstruction instruction,
        HashSet<MirLocalId> writes
    )
    {
        switch (instruction)
        {
            case MirAssignInstruction assign:
                CollectDestinationWrite(assign.Destination, writes);
                break;
            case MirBinaryInstruction binary:
                CollectDestinationWrite(binary.Destination, writes);
                break;
            case MirCallInstruction call:
                if (call.Destination is not null)
                {
                    CollectDestinationWrite(call.Destination, writes);
                }
                break;
            case MirConvertInstruction convert:
                CollectDestinationWrite(convert.Destination, writes);
                break;
            case MirUnaryInstruction unary:
                CollectDestinationWrite(unary.Destination, writes);
                break;
        }
    }

    private static void CollectDestinationWrite(MirPlace place, HashSet<MirLocalId> writes)
    {
        if (place is MirLocalPlace local)
            writes.Add(local.LocalId);
    }

    private static void RecordTerminator(
        MirTerminator terminator,
        ImmutableHashSet<MirLocalId>.Builder uses,
        ImmutableHashSet<MirLocalId>.Builder allReads,
        HashSet<MirLocalId> definedSoFar
    )
    {
        var reads = new HashSet<MirLocalId>();
        CollectTerminatorReads(terminator, reads);

        foreach (var read in reads)
        {
            allReads.Add(read);
            if (!definedSoFar.Contains(read))
                uses.Add(read);
        }
    }

    private static void CollectTerminatorReads(MirTerminator terminator, HashSet<MirLocalId> reads)
    {
        switch (terminator)
        {
            case MirBranchTerminator branch:
                CollectValueReads(branch.Condition, reads);
                return;

            case MirReturnTerminator { Value: not null } ret:
                CollectValueReads(ret.Value, reads);
                return;

            case MirGotoTerminator:
            case MirReturnTerminator:
            case MirUnreachableTerminator:
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(terminator));
        }
    }

    public static MirLocalFlowAnalysis AnalyzeLocalFlow(
        MirFunction function,
        MirControlFlowGraph cfg,
        MirUseDefAnalysis useDef
    )
    {
        var builders = function.Locals.ToDictionary(
            l => l.Id,
            l => new MirLocalFlowInfoBuilder(l.Id)
        );

        SeedLocalBlockFacts(builders, useDef);

        foreach (var block in cfg.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                AnalyzeInstructionLocalFlow(instruction, block.Id, builders);
            }

            AnalyzeTerminatorLocalFlow(block.Terminator, block.Id, builders);
        }

        foreach (var builder in builders.Values)
        {
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
            case MirAssignInstruction assign:
                AnalyzeWriteDestination(assign.Destination, blockId, builders);
                AnalyzeValue(assign.Source, blockId, builders);
                break;

            case MirUnaryInstruction unary:
                AnalyzeWriteDestination(unary.Destination, blockId, builders);
                AnalyzeValue(unary.Value, blockId, builders);
                break;

            case MirBinaryInstruction binary:
                AnalyzeWriteDestination(binary.Destination, blockId, builders);
                AnalyzeValue(binary.Left, blockId, builders);
                AnalyzeValue(binary.Right, blockId, builders);
                break;

            case MirConvertInstruction convert:
                AnalyzeWriteDestination(convert.Destination, blockId, builders);
                AnalyzeValue(convert.Value, blockId, builders);
                break;

            case MirCallInstruction call:
                if (call.Destination is not null)
                    AnalyzeWriteDestination(call.Destination, blockId, builders);

                foreach (var argument in call.Arguments)
                    AnalyzeValue(argument, blockId, builders);
                break;

            case MirStorageLiveInstruction:
            case MirStorageDeadInstruction:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(instruction));
        }
    }

    private static void SeedLocalBlockFacts(
        Dictionary<MirLocalId, MirLocalFlowInfoBuilder> builders,
        MirUseDefAnalysis useDef
    )
    {
        foreach (var blockInfo in useDef.Blocks.Values)
        {
            foreach (var localId in blockInfo.Defs)
            {
                builders[localId].DefBlocks.Add(blockInfo.BlockId);
            }

            foreach (var localId in blockInfo.AllReads)
            {
                builders[localId].UseBlocks.Add(blockInfo.BlockId);
            }
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
                return;

            case MirReadValue read:
                AnalyzeReadFromPlace(read.Place, blockId, builders);
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

            case MirFieldPlace field:
                AnalyzeReadFromPlace(field.Base, blockId, builders);
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

            case MirFieldPlace field:
                AnalyzePlaceComputation(field.Base, blockId, builders);
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

            case MirFieldPlace field:
                AnalyzeAddressOfPlace(field.Base, builders);
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
        builder.HasMergePotential = HasMergePotential(builder, cfg);
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

    private static bool HasMergePotential(MirLocalFlowInfoBuilder builder, MirControlFlowGraph cfg)
    {
        if (builder.DefBlocks.Count < 2)
            return false;

        foreach (var block in cfg.Blocks)
        {
            if (
                !cfg.Predecessors.TryGetValue(block.Id, out var predecessors)
                || predecessors.Length < 2
            )
                continue;

            var reachingDefCount = 0;
            foreach (
                var _ in builder
                    .DefBlocks.AsValueEnumerable()
                    .Where(defBlock => CanReach(defBlock, block.Id, cfg))
            )
            {
                reachingDefCount++;
                if (reachingDefCount >= 2)
                    return true;
            }
        }

        return false;
    }

    private static bool CanReach(MirBlockId from, MirBlockId to, MirControlFlowGraph cfg)
    {
        if (from == to)
            return true;

        var visited = new HashSet<MirBlockId>();
        var queue = new Queue<MirBlockId>();
        queue.Enqueue(from);

        while (queue.TryDequeue(out var current))
        {
            if (!visited.Add(current))
                continue;

            if (!cfg.Successors.TryGetValue(current, out var successors))
                continue;

            foreach (var successor in successors)
            {
                if (successor == to)
                    return true;

                if (!visited.Contains(successor))
                    queue.Enqueue(successor);
            }
        }

        return false;
    }

    public static MirLocalClassificationAnalysis ClassifyLocals(MirLocalFlowAnalysis localFlow)
    {
        var builder = ImmutableDictionary.CreateBuilder<MirLocalId, MirLocalClassification>();

        foreach (var pair in localFlow.Locals)
        {
            var local = pair.Value;
            builder.Add(pair.Key, ClassifyLocal(local));
        }

        return new MirLocalClassificationAnalysis { Locals = builder.ToImmutable() };
    }

    private static MirLocalClassification ClassifyLocal(MirLocalFlowInfo local)
    {
        if (local.IsAddressTaken)
        {
            return new MirLocalClassification
            {
                LocalId = local.LocalId,
                StorageKind = MirLocalStorageKind.Memory,
                IsSsaEligible = false,
            };
        }

        if (local.HasMergePotential)
        {
            return new MirLocalClassification
            {
                LocalId = local.LocalId,
                StorageKind = MirLocalStorageKind.SsaWithPhi,
                IsSsaEligible = true,
            };
        }

        return new MirLocalClassification
        {
            LocalId = local.LocalId,
            StorageKind = MirLocalStorageKind.Ssa,
            IsSsaEligible = true,
        };
    }
}
