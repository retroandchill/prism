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
