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

    public static FrozenDictionary<Local, LocalUsageInfo> AnalyzeLocalFlow(
        Function function,
        CancellationToken cancellationToken
    )
    {
        var builders = function.Locals.ToDictionary(
            local => local,
            _ => new LocalUsageInfoBuilder()
        );

        foreach (var block in function.Blocks)
        {
            foreach (var instruction in block.Statements)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AnalyzeInstructionLocalFlow(instruction, builders);
            }

            cancellationToken.ThrowIfCancellationRequested();
            AnalyzeTerminatorLocalFlow(block.Terminator, builders);
        }

        return builders.ToFrozenDictionary(t => t.Key, t => t.Value.Build());
    }

    private static void AnalyzeInstructionLocalFlow(
        Statement instruction,
        Dictionary<Local, LocalUsageInfoBuilder> builders
    )
    {
        switch (instruction)
        {
            case AssignmentStatement assignmentStatement:
                AnalyzePlaceUsage(assignmentStatement.Target, builders, false);
                AnalyzeRValue(assignmentStatement.Value, builders);
                break;
            case StorageDeadStatement:
            case StorageLiveStatement:
                break;
        }
    }

    private static void AnalyzeTerminatorLocalFlow(
        Terminator terminator,
        Dictionary<Local, LocalUsageInfoBuilder> builders
    )
    {
        switch (terminator)
        {
            case BranchTerminator(var condition, _, _):
                AnalyzeOperand(condition, builders);
                break;
            case CallTerminator(_, var receiver, var arguments, var destination, _, _):
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
                    AnalyzePlaceUsage(dest, builders, false);
                }

                break;
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

        if (isTakingAddress)
        {
            usageInfo.IsAddressed = true;
        }

        if (usageInfo.IsProjected)
            return;

        if (place.Projections.IsEmpty)
            return;

        // If the first projection is a dereference, we don't consider it a projection, otherwise we are accessing
        // part of a place, which means we may need to be mutating parts individually, and thus we need to note that
        // there may be a need for heap storage.
        if (place.Projections[0] is DereferenceProjection)
            return;

        usageInfo.IsProjected = true;
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
            case IsNull(var operand):
                AnalyzeOperand(operand, builders);
                break;
            case IsNotNull(var operand):
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
