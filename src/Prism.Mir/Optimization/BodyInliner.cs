using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ZLinq;

namespace Prism.Mir.Optimization;

public static class BodyInliner
{
    public static BasicBlockBuilder InlineBody(
        ExecutableBody source,
        Place returnDestination,
        ExecutableBodyBuilder target,
        BasicBlockBuilder currentBlock,
        CancellationToken cancellationToken
    )
    {
        return InlineBody(source, returnDestination, target, currentBlock, null, cancellationToken);
    }

    public static BasicBlockBuilder InlineBody(
        ExecutableBody source,
        Place returnDestination,
        ExecutableBodyBuilder target,
        BasicBlockBuilder currentBlock,
        IEnumerable<KeyValuePair<Local, Local>>? externalLocals = null,
        CancellationToken cancellationToken = default
    )
    {
        var newLocals = new Dictionary<Local, Local>();
        foreach (var (oldLocal, newLocal) in externalLocals ?? [])
        {
            newLocals.Add(oldLocal, newLocal);
        }

        foreach (
            var local in source
                .Locals.AsValueEnumerable()
                .Where(local => local.Id != LocalId.ReturnSlot)
        )
        {
            if (newLocals.ContainsKey(local))
                continue;

            var newLocal = target.AddLocal(local.Type);
            newLocals.Add(local, newLocal);
        }

        var blockMapping = new Dictionary<BasicBlock, BasicBlockBuilder>();
        foreach (var block in source.Blocks)
        {
            var newBlock = target.AddBasicBlock(block.Name);
            blockMapping.Add(block, newBlock);
        }

        var joinBlock = target.AddBasicBlock("join");

        currentBlock.SetGotoTerminator(blockMapping[source.EntryBlock]);

        foreach (var block in source.Blocks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var targetBlock = blockMapping[block];

            foreach (var instruction in block.Instructions)
            {
                switch (instruction)
                {
                    case AssignmentInstruction(var targetPlace, var rvalue):
                        targetBlock.AddAssignment(VisitPlace(targetPlace), VisitRValue(rvalue));
                        break;
                    case StorageLiveInstruction(var local):
                        targetBlock.AddStorageLive(newLocals[local]);
                        break;
                    case StorageDeadInstruction(var local):
                        targetBlock.AddStorageDead(newLocals[local]);
                        break;
                }
            }

            switch (block.Terminator)
            {
                case BranchTerminator(var condition, var whenTrue, var whenFalse):
                    targetBlock.SetBranchTerminator(
                        VisitOperand(condition),
                        blockMapping[whenTrue],
                        blockMapping[whenFalse]
                    );
                    break;
                case CallTerminator(
                    var function,
                    var receiver,
                    var arguments,
                    var destination,
                    var nextBlock,
                    var unwindBlock
                ):
                    targetBlock.SetCallTerminator(
                        function,
                        VisitOperand(receiver),
                        VisitOperands(arguments),
                        TryVisitPlace(destination),
                        blockMapping[nextBlock],
                        unwindBlock is not null ? blockMapping[unwindBlock] : null
                    );
                    break;
                case GotoTerminator(var dest):
                    targetBlock.SetGotoTerminator(blockMapping[dest]);
                    break;
                case ReturnTerminator:
                    targetBlock.SetGotoTerminator(joinBlock);
                    break;
                case UnreachableTerminator:
                    targetBlock.SetUnreachable();
                    break;
            }
        }

        return joinBlock;

        Place VisitPlace(Place place)
        {
            return place.Base switch
            {
                LocalPlace(var local) when local.Id == LocalId.ReturnSlot =>
                    returnDestination.AddProjections(VisitPlaceProjections(place.Projections)),
                LocalPlace(var local) => new Place(
                    new LocalPlace(newLocals[local]),
                    VisitPlaceProjections(place.Projections)
                ),
                GlobalPlace => place with
                {
                    Projections = VisitPlaceProjections(place.Projections),
                },
            };
        }

        Place? TryVisitPlace(Place? place)
        {
            return place is not null ? VisitPlace(place.Value) : (Place?)null;
        }

        ImmutableArray<PlaceProjection> VisitPlaceProjections(
            ImmutableArray<PlaceProjection> projections
        )
        {
            PlaceProjection[]? copy = null;
            foreach (var (i, projection) in projections.AsValueEnumerable().Index())
            {
                var newProjection = VisitPlaceProjection(projection);
                if (projection == newProjection)
                {
                    copy?[i] = newProjection;
                }
                else
                {
                    copy = new PlaceProjection[projections.Length];
                    projections.CopyTo(copy, i);
                    copy[i] = newProjection;
                }
            }

            return copy is null ? projections : ImmutableCollectionsMarshal.AsImmutableArray(copy);
        }

        PlaceProjection VisitPlaceProjection(PlaceProjection projection)
        {
            return projection switch
            {
                IndexProjection index => index.WithIndex(VisitOperand(index.Index)),
                PayloadProjection or DereferenceProjection or FieldProjection => projection,
            };
        }

        [return: NotNullIfNotNull(nameof(operand))]
        Operand? VisitOperand(Operand? operand)
        {
            return operand switch
            {
                null => null,
                ConstantOperand constant => constant,
                PlaceOperand place => place.WithSource(VisitPlace(place.Source)),
            };
        }

        ImmutableArray<Operand> VisitOperands(ImmutableArray<Operand> operands)
        {
            Operand[]? copy = null;
            foreach (var (i, operand) in operands.AsValueEnumerable().Index())
            {
                var newOperand = VisitOperand(operand);
                if (operand == newOperand)
                {
                    copy?[i] = newOperand;
                }
                else
                {
                    copy = new Operand[operands.Length];
                    operands.CopyTo(copy, i);
                    copy[i] = newOperand;
                }
            }

            return copy is null ? operands : ImmutableCollectionsMarshal.AsImmutableArray(copy);
        }

        RValue VisitRValue(RValue rvalue)
        {
            return rvalue switch
            {
                UseRValue use => use.WithOperand(VisitOperand(use.Operand)),
                UnaryRValue unary => unary.WithOperand(VisitOperand(unary.Operand)),
                BinaryRValue binary => binary.WithOperands(
                    VisitOperand(binary.Left),
                    VisitOperand(binary.Right)
                ),
                IsNullRValue isNull => isNull.WithOperand(VisitOperand(isNull.Operand)),
                IsNotNullRValue isNotNull => isNotNull.WithOperand(VisitOperand(isNotNull.Operand)),
                SelectRValue select => select.WithOperands(
                    VisitOperand(select.Condition),
                    VisitOperand(select.TrueValue),
                    VisitOperand(select.FalseValue)
                ),
                ConversionRValue conversion => conversion.WithOperand(conversion.Operand),
                ReferenceRValue reference => reference.WithPlace(VisitPlace(reference.Place)),
                ArrayRValue array => array.WithElements(VisitOperands(array.Elements)),
                DataRValue data => data.WithOperand(data.Operand),
                LengthRValue length => length.WithOperand(VisitOperand(length.Operand)),
            };
        }
    }
}
