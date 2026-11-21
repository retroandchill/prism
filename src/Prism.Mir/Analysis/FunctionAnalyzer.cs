using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Abi;
using Prism.Utils;

namespace Prism.Mir.Analysis;

using LocalUsageMap = FrozenDictionary<Local, LocalUsageInfo>;

public static class FunctionAnalyzer
{
    private static readonly ConditionalWeakTable<Function, LocalUsageMap> LocalUsages = [];

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

    public static LocalUsageMap AnalyzeLocalFlow(
        Function function,
        IAbiLookup abiLookup,
        CancellationToken cancellationToken
    )
    {
        return LocalUsages.GetOrAdd(
            function,
            static (function, t) =>
            {
                var builders = function.Locals.ToDictionary(
                    local => local,
                    _ => new LocalUsageInfoBuilder()
                );

                foreach (var block in function.Blocks)
                {
                    foreach (var instruction in block.Instructions)
                    {
                        t.cancellationToken.ThrowIfCancellationRequested();
                        AnalyzeInstructionLocalFlow(instruction, builders);
                    }

                    t.cancellationToken.ThrowIfCancellationRequested();
                    AnalyzeTerminatorLocalFlow(block.Terminator, t.abiLookup, builders);
                }

                return builders.ToFrozenDictionary(t => t.Key, t => t.Value.Build());
            },
            (abiLookup, cancellationToken)
        );
    }

    private static void AnalyzeInstructionLocalFlow(
        Instruction instruction,
        Dictionary<Local, LocalUsageInfoBuilder> builders
    )
    {
        switch (instruction)
        {
            case AssignmentInstruction(var place, var value) assign:
                if (place.Base is LocalPlace(var local))
                {
                    builders[local].Assignments.Add(assign);
                }

                AnalyzePlaceUsage(place, builders, false);
                AnalyzeRValue(value, builders);
                break;
            case StorageDeadInstruction:
            case StorageLiveInstruction:
                break;
        }
    }

    private static void AnalyzeTerminatorLocalFlow(
        Terminator terminator,
        IAbiLookup abiLookup,
        Dictionary<Local, LocalUsageInfoBuilder> builders
    )
    {
        switch (terminator)
        {
            case BranchTerminator(var condition, _, _):
                AnalyzeOperand(condition, builders);
                break;
            case CallTerminator(var function, var receiver, var arguments, var destination, _, _):
            {
                if (receiver is not null)
                {
                    AnalyzeOperand(receiver, builders);
                }

                foreach (var argument in arguments)
                {
                    AnalyzeOperand(argument, builders);
                }

                if (destination is { } dest)
                {
                    // If we have an indirect return, we need to analyze the destination as a write
                    var abi = abiLookup.GetFunctionAbi(function);
                    AnalyzePlaceUsage(dest, builders, false, abi.Return.IsIndirect);
                }

                break;
            }
            case GotoTerminator:
            case ReturnTerminator:
            case UnreachableTerminator:
                break;
        }
    }

    private static void AnalyzePlaceUsage(
        Place place,
        Dictionary<Local, LocalUsageInfoBuilder> builders,
        bool isRead,
        bool isTakingAddress = false
    )
    {
        foreach (var indexer in place.Projections.OfType<IndexProjection>())
        {
            AnalyzeOperand(indexer.Index, builders);
        }

        if (place.Base is not LocalPlace(var local))
            return;

        var usageInfo = builders[local];

        if (isRead)
        {
            usageInfo.ReadCount++;
        }
        else
        {
            usageInfo.WriteCount++;
        }

        if (!place.Projections.IsEmpty)
        {
            // If the first projection is a dereference, we don't consider it a projection, otherwise we are accessing
            // part of a place, which means we may need to be mutating parts individually, and thus we need to note that
            // there may be a need for heap storage.
            if (place.Projections[0] is DereferenceProjection)
                return;

            usageInfo.Projection |= isRead ? ProjectionUsage.Read : ProjectionUsage.Write;
        }

        if (isTakingAddress)
        {
            usageInfo.IsAddressed = true;
        }
    }

    private static void AnalyzeOperand(
        Operand operand,
        Dictionary<Local, LocalUsageInfoBuilder> builders
    )
    {
        switch (operand)
        {
            case ConstantOperand:
                break;
            case PlaceOperand(var place):
                AnalyzePlaceUsage(place, builders, true);
                break;
        }
    }

    private static void AnalyzeRValue(
        RValue rValue,
        Dictionary<Local, LocalUsageInfoBuilder> builders
    )
    {
        switch (rValue)
        {
            case UseRValue(var operand):
                AnalyzeOperand(operand, builders);
                break;
            case UnaryRValue(_, var operand):
                AnalyzeOperand(operand, builders);
                break;
            case BinaryRValue(_, var leftOperand, var rightOperand):
                AnalyzeOperand(leftOperand, builders);
                AnalyzeOperand(rightOperand, builders);
                break;
            case IsNullRValue(var operand):
                AnalyzeOperand(operand, builders);
                break;
            case IsNotNullRValue(var operand):
                AnalyzeOperand(operand, builders);
                break;
            case SelectRValue(var condition, var whenTrue, var whenFalse):
                AnalyzeOperand(condition, builders);
                AnalyzeOperand(whenTrue, builders);
                AnalyzeOperand(whenFalse, builders);
                break;
            case ConversionRValue(var operand, _, _):
                AnalyzeOperand(operand, builders);
                break;
            case ReferenceRValue(var place, _):
                AnalyzePlaceUsage(place, builders, true, true);
                break;
            case ArrayRValue(var elements):
                foreach (var element in elements)
                {
                    AnalyzeOperand(element, builders);
                }
                break;
            case DataRValue(var operand):
                AnalyzeOperand(operand, builders);
                break;
            case LengthRValue(var operand):
                AnalyzeOperand(operand, builders);
                break;
        }
    }
}
