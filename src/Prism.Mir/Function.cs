// @file MirFunction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols;
using Riok.Mapperly.Abstractions;
using ZLinq;

namespace Prism.Mir;

public sealed partial class Function
{
    public FunctionSymbol Symbol { get; }

    public ImmutableArray<BasicBlock> Blocks { get; private set; }

    public BasicBlock EntryBlock { get; private set; } = null!;

    public ImmutableArray<Local> Locals { get; private set; }

    private Function(FunctionSymbol symbol)
    {
        Symbol = symbol;
    }

    public Function PruneUnusedBlocksAndLocals()
    {
        var usedLocals = new HashSet<Local>();

        var explored = new HashSet<BasicBlock>();
        var toExplore = new Queue<BasicBlock>();
        toExplore.Enqueue(EntryBlock);
        while (toExplore.TryDequeue(out var block))
        {
            if (!explored.Add(block))
                continue;

            foreach (var instruction in block.Statements)
            {
                switch (instruction)
                {
                    case AssignmentStatement(var place, var target):
                        CheckPlace(place);
                        CheckRValue(target);
                        break;
                    case StorageDeadStatement(var local):
                        usedLocals.Add(local);
                        break;
                    case StorageLiveStatement(var local):
                        usedLocals.Add(local);
                        break;
                }
            }

            switch (block.Terminator)
            {
                case BranchTerminator(var condition, var trueBlock, var falseBlock):
                    CheckOperand(condition);
                    toExplore.Enqueue(trueBlock);
                    toExplore.Enqueue(falseBlock);
                    break;
                case CallTerminator(
                    _,
                    var receiver,
                    var arguments,
                    var destination,
                    var nextBlock,
                    var unwindBlock
                ):

                    if (receiver is not null)
                        CheckOperand(receiver);

                    foreach (var argument in arguments)
                        CheckOperand(argument);

                    if (destination is not null)
                        CheckPlace(destination.Value);
                    toExplore.Enqueue(nextBlock);
                    if (unwindBlock is not null)
                        toExplore.Enqueue(unwindBlock);
                    break;
                case GotoTerminator(var nextBlock):
                    toExplore.Enqueue(nextBlock);
                    break;
                case ReturnTerminator:
                case UnreachableTerminator:
                    break;
            }
        }

        var locals = Locals.RemoveAll(local => !usedLocals.Contains(local));
        var blocks = Blocks.RemoveAll(block => !toExplore.Contains(block));

        if (locals == Locals && blocks == Blocks)
            return this;

        return new Function(Symbol)
        {
            Blocks = blocks,
            EntryBlock = EntryBlock,
            Locals = locals,
        };

        void CheckRValue(RValue rvalue)
        {
            switch (rvalue)
            {
                case UseRValue(var operand):
                    CheckOperand(operand);
                    break;
                case UnaryRValue(_, var operand):
                    CheckOperand(operand);
                    break;
                case BinaryRValue(_, var leftOperand, var rightOperand):
                    CheckOperand(leftOperand);
                    CheckOperand(rightOperand);
                    break;
                case IsNull(var operand):
                    CheckOperand(operand);
                    break;
                case IsNotNull(var operand):
                    CheckOperand(operand);
                    break;
                case SelectRValue(var condition, var whenTrue, var whenFalse):
                    CheckOperand(condition);
                    CheckOperand(whenTrue);
                    CheckOperand(whenFalse);
                    break;
                case ConversionRValue(var operand, _, _):
                    CheckOperand(operand);
                    break;
                case ReferenceRValue(var place, _):
                    CheckPlace(place);
                    break;
                case ArrayRValue(var elements):
                    foreach (var element in elements)
                    {
                        CheckOperand(element);
                    }
                    break;
                case DataRValue(var operand):
                    CheckOperand(operand);
                    break;
                case LengthRValue(var operand):
                    CheckOperand(operand);
                    break;
            }
        }

        void CheckOperand(Operand operand)
        {
            if (operand is PlaceOperand(var place))
                CheckPlace(place);
        }

        void CheckPlace(Place place)
        {
            if (place.Base is LocalPlace(var local))
                usedLocals.Add(local);
        }
    }

    internal void Validate()
    {
        var blockSet = Blocks.AsValueEnumerable().ToHashSet();
        var localsSet = Locals.AsValueEnumerable().ToHashSet();

        foreach (var block in Blocks)
        {
            switch (block.Terminator)
            {
                case BranchTerminator(var condition, var trueBlock, var falseBlock):
                    ValidateOperand(condition);
                    ValidateBlock(trueBlock);
                    ValidateBlock(falseBlock);
                    if (trueBlock == falseBlock)
                    {
                        throw new InvalidOperationException(
                            "Branch terminator with identical true and false blocks"
                        );
                    }
                    break;
                case CallTerminator(
                    _,
                    var receiver,
                    var arguments,
                    var destination,
                    var nextBlock,
                    var unwindBlock
                ):
                    if (receiver is not null)
                        ValidateOperand(receiver);
                    foreach (var argument in arguments)
                        ValidateOperand(argument);

                    if (destination is { } notNullPlace)
                        ValidatePlace(notNullPlace);

                    ValidateBlock(nextBlock);
                    if (unwindBlock is not null)
                        ValidateBlock(unwindBlock);

                    if (nextBlock == unwindBlock)
                    {
                        throw new InvalidOperationException(
                            "Branch terminator with identical next and unwind blocks"
                        );
                    }
                    break;
                case GotoTerminator(var nextBlock):
                    ValidateBlock(nextBlock);
                    break;
                case ReturnTerminator:
                case UnreachableTerminator:
                    break;
            }
        }

        return;

        void ValidateBlock(BasicBlock block)
        {
            if (!blockSet.Contains(block))
                throw new InvalidMirException($"Block {block} is not in the set of blocks");
        }

        void ValidateOperand(Operand operand)
        {
            if (operand is PlaceOperand(var place))
                ValidatePlace(place);
        }

        void ValidatePlace(Place place)
        {
            if (place.Base is LocalPlace(var local))
                ValidateLocal(local);
        }

        void ValidateLocal(Local local)
        {
            if (!localsSet.Contains(local))
                throw new InvalidMirException($"Local {local} is not in the set of locals");
        }
    }

    [Mapper(UseReferenceHandling = true, RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    [UseStaticMapper(typeof(BasicBlock.Mapper))]
    internal static partial class Mapper
    {
        public static partial Function Map(FunctionBuilder builder);
    }
}
