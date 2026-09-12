// @file MirEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Mappers;
using Prism.Core.Mir;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen.Mir;

internal sealed class MirEmitter(Compilation compilation)
{
    private enum UnaryResultKind : byte
    {
        Prefix,
        Postfix,
    }

    private enum UnaryArithmeticKind : byte
    {
        Increment,
        Decrement,
    }

    private enum LogicalOperation : byte
    {
        And,
        Or,
    }

    private readonly MirNullValue _nullValue = new(compilation.GetSpecialType(SpecialType.Void));

    private readonly MirConstantValue _falseValue = new(
        ConstantValue.Boolean(false),
        compilation.GetSpecialType(SpecialType.Bool)
    );
    private readonly MirConstantValue _trueValue = new(
        ConstantValue.Boolean(true),
        compilation.GetSpecialType(SpecialType.Bool)
    );

    public MirFunction EmitFunction(FunctionSymbol symbol, CancellationToken cancellationToken)
    {
        var body = compilation.GetBoundBody(symbol, cancellationToken);
        var builder = new MirFunctionBuilder(symbol);

        var context = new MirEmissionContext(builder);

        if (body.Body is not null)
        {
            var entry = builder.AddBlock("entry");
            builder.SetEntryBlock(entry.Id);
            context.SetCurrentBlock(entry);

            EmitStatement(body.Body, context, cancellationToken);

            if (!context.CurrentBlock.IsTerminated && symbol.ReturnsVoid)
            {
                context.CurrentBlock.SetTerminator(MirReturnTerminator.Void);
            }
        }
        else
        {
            var entry = builder.AddBlock("entry");
            builder.SetEntryBlock(entry.Id);
            entry.SetTerminator(MirUnreachableTerminator.Instance);
        }

        return builder.Build();
    }

    private void EmitStatement(
        BoundStatement statement,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (statement)
        {
            case BoundBlock boundBlock:
                EmitBlock(boundBlock, context, cancellationToken);
                break;
            case BoundVariableDeclaration boundVariableDeclaration:
                EmitLocal(boundVariableDeclaration, context, cancellationToken);
                break;
            case BoundExpressionStatement boundExpressionStatement:
                EmitExpressionStatement(boundExpressionStatement, context, cancellationToken);
                break;
            case BoundReturnStatement boundReturnStatement:
                EmitReturn(boundReturnStatement, context, cancellationToken);
                break;
            case BoundIfStatement boundIfStatement:
                EmitIfStatement(boundIfStatement, context, cancellationToken);
                break;
            case BoundWhileStatement boundWhileStatement:
                EmitWhileStatement(boundWhileStatement, context, cancellationToken);
                break;
            case BoundLoopStatement boundLoopStatement:
                EmitLoopStatement(boundLoopStatement, context, cancellationToken);
                break;
            case BoundForStatement boundForStatement:
                EmitForLoop(boundForStatement, context, cancellationToken);
                break;
            case BoundBreakStatement boundBreakStatement:
                EmitBreakStatement(boundBreakStatement, context);
                break;
            case BoundContinueStatement boundContinueStatement:
                EmitContinueStatement(boundContinueStatement, context);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private void EmitBlock(
        BoundBlock block,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        foreach (var statement in block.Statements)
        {
            EmitStatement(statement, context, cancellationToken);
        }
    }

    private void EmitLocal(
        BoundVariableDeclaration declaration,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var local = context.BindLocal(declaration.Variable);
        if (declaration.Initializer is null)
            return;

        var operand = EmitExpression(declaration.Initializer, context, cancellationToken);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(new MirLocalPlace(local.Id, local.Type), operand)
        );
    }

    private void EmitExpressionStatement(
        BoundExpressionStatement expression,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        _ = EmitExpression(expression.Expression, context, cancellationToken);
    }

    private void EmitReturn(
        BoundReturnStatement statement,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        if (statement.Expression is not null)
        {
            var result = EmitExpression(statement.Expression, context, cancellationToken);
            context.CurrentBlock.SetTerminator(new MirReturnTerminator(result));
        }
        else
        {
            context.CurrentBlock.SetTerminator(MirReturnTerminator.Void);
        }
    }

    private void EmitIfStatement(
        BoundIfStatement statement,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var thenBlock = context.AddBlock("then.cond");
        var elseBlock = statement.ElseStatement is not null
            ? context.AddDetachedBlock("else.cond")
            : null;
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        var condition = EmitExpression(statement.Condition, context, cancellationToken);
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, thenBlock.Id, elseBlock?.Id ?? mergeBlock.Id)
        );

        var thenFallsThrough = false;
        context.SetCurrentBlock(thenBlock);
        EmitStatement(statement.ThenStatement, context, cancellationToken);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));
            thenFallsThrough = true;
        }

        var elseFallsThrough = false;
        if (elseBlock is not null)
        {
            context.AddBlock(elseBlock);
            context.SetCurrentBlock(elseBlock);
            Debug.Assert(statement.ElseStatement is not null);
            EmitStatement(statement.ElseStatement, context, cancellationToken);
            if (!context.CurrentBlock.IsTerminated)
            {
                context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));
                elseFallsThrough = true;
            }
        }

        if (!thenFallsThrough && !elseFallsThrough && statement.ElseStatement is not null)
            return;

        context.AddBlock(mergeBlock);
        context.SetCurrentBlock(mergeBlock);
    }

    private void EmitWhileStatement(
        BoundWhileStatement statement,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var loopHead = context.AddBlock("loop.head");
        var loopBody = context.AddDetachedBlock("loop.body");
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(statement.Label, loopTail.Id, loopHead.Id);

        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        context.SetCurrentBlock(loopHead);
        var condition = EmitExpression(statement.Condition, context, cancellationToken);
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, loopBody.Id, loopTail.Id)
        );

        context.AddBlock(loopBody);
        context.SetCurrentBlock(loopBody);
        EmitStatement(statement.Body, context, cancellationToken);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        }

        context.AddBlock(loopTail);
        context.SetCurrentBlock(loopTail);
    }

    private void EmitLoopStatement(
        BoundLoopStatement statement,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var loopHead = context.AddBlock("loop.head");
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(statement.Label, loopTail.Id, loopHead.Id);

        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        context.SetCurrentBlock(loopHead);
        EmitStatement(statement.Body, context, cancellationToken);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        }

        context.AddBlock(loopTail);
        context.SetCurrentBlock(loopTail);
    }

    private void EmitForLoop(
        BoundForStatement loop,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        if (loop.Variable is not null)
        {
            EmitLocal(loop.Variable, context, cancellationToken);
        }

        foreach (var initializer in loop.Initializers)
        {
            EmitExpression(initializer, context, cancellationToken);
        }

        var loopHead = context.AddBlock("loop.head");
        var loopBody = loop.Condition is not null ? context.AddDetachedBlock("loop.body") : null;
        var loopIncrement =
            loop.Incrementors.Length > 0 ? context.AddDetachedBlock("loop.increment") : null;
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(loop.Label, loopTail.Id, loopIncrement?.Id ?? loopHead.Id);

        context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        context.SetCurrentBlock(loopHead);
        if (loopBody is not null)
        {
            Debug.Assert(loop.Condition is not null);
            var condition = EmitExpression(loop.Condition, context, cancellationToken);
            context.CurrentBlock.SetTerminator(
                new MirBranchTerminator(condition, loopBody.Id, loopTail.Id)
            );

            context.AddBlock(loopBody);
            context.SetCurrentBlock(loopBody);
        }

        EmitStatement(loop.Body, context, cancellationToken);

        if (!context.CurrentBlock.IsTerminated)
        {
            if (loopIncrement is not null)
            {
                context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopIncrement.Id));

                context.AddBlock(loopIncrement);
                context.SetCurrentBlock(loopIncrement);
                foreach (var incrementor in loop.Incrementors)
                {
                    EmitExpression(incrementor, context, cancellationToken);
                }
            }

            context.CurrentBlock.SetTerminator(new MirGotoTerminator(loopHead.Id));
        }

        context.AddBlock(loopTail);
        context.SetCurrentBlock(loopTail);
    }

    private static void EmitBreakStatement(
        BoundBreakStatement statement,
        MirEmissionContext context
    )
    {
        var (breakTarget, _) = context.GetLoopTargets(statement.Label);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(breakTarget));
    }

    private static void EmitContinueStatement(
        BoundContinueStatement statement,
        MirEmissionContext context
    )
    {
        var (_, continueTarget) = context.GetLoopTargets(statement.Label);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(continueTarget));
    }

    private MirValue EmitExpression(
        BoundExpression expression,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (expression.ConstantValue is { } constantValue)
        {
            return new MirConstantValue(constantValue, expression.Type);
        }

        return expression switch
        {
            BoundVariableAccess access => EmitAccess(access, context),
            BoundParameterAccess access => EmitAccess(access, context),
            BoundUnaryOperation unary => EmitOperation(unary, context, cancellationToken),
            BoundBinaryOperation binary => EmitOperation(binary, context, cancellationToken),
            BoundAssignmentOperation assignment => EmitAssignment(
                assignment,
                context,
                cancellationToken
            ),
            BoundConditional conditional => EmitConditional(
                conditional,
                context,
                cancellationToken
            ),
            BoundInvocation invocation => EmitCall(invocation, context, cancellationToken),
            BoundConversion conversion => EmitConversion(conversion, context, cancellationToken),
            BoundAddressOf addressOf => EmitAddressOf(addressOf, context, cancellationToken),
            BoundDereference dereference => EmitDereference(
                dereference,
                context,
                cancellationToken
            ),
            BoundIndex index => EmitIndex(index, context, cancellationToken),
            BoundBadExpression or BoundSpeculativeExpression => throw new InvalidOperationException(
                "Should only emit LLVM IR if the compilation is valid"
            ),
            BoundLiteral => throw new InvalidOperationException(
                "The constant folding should have grabbed this already"
            ),
            _ => throw new InvalidOperationException("We probably added a new expression type"),
        };
    }

    private static MirReadValue EmitAccess(BoundVariableAccess access, MirEmissionContext context)
    {
        if (context.TryGetLocal(access.Symbol) is { } local)
        {
            return new MirReadValue(new MirLocalPlace(local.Id, local.Type));
        }

        return new MirReadValue(new MirGlobalPlace(access.Symbol));
    }

    private static MirReadValue EmitAccess(BoundParameterAccess access, MirEmissionContext context)
    {
        var local = context.GetLocal(access.Symbol);
        return new MirReadValue(new MirLocalPlace(local.Id, local.Type));
    }

    private MirValue EmitOperation(
        BoundUnaryOperation operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        return operation.Operation switch
        {
            UnaryOperation.Identity => EmitExpression(
                operation.Operand,
                context,
                cancellationToken
            ),

            UnaryOperation.Negation => EmitSimpleUnary(
                operation,
                MirUnaryOp.Negation,
                context,
                cancellationToken
            ),

            UnaryOperation.LogicalNot => EmitSimpleUnary(
                operation,
                MirUnaryOp.LogicalNot,
                context,
                cancellationToken
            ),

            UnaryOperation.BitwiseNot => EmitSimpleUnary(
                operation,
                MirUnaryOp.BitwiseNot,
                context,
                cancellationToken
            ),

            UnaryOperation.PreIncrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Prefix,
                UnaryArithmeticKind.Increment,
                context,
                cancellationToken
            ),

            UnaryOperation.PreDecrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Prefix,
                UnaryArithmeticKind.Decrement,
                context,
                cancellationToken
            ),

            UnaryOperation.PostIncrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Postfix,
                UnaryArithmeticKind.Increment,
                context,
                cancellationToken
            ),

            UnaryOperation.PostDecrement => EmitUnaryIncrementDecrement(
                operation,
                UnaryResultKind.Postfix,
                UnaryArithmeticKind.Decrement,
                context,
                cancellationToken
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
    }

    private MirReadValue EmitSimpleUnary(
        BoundUnaryOperation operation,
        MirUnaryOp mirOp,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var temp = context.CreateTemp(operation.Type);
        var place = new MirLocalPlace(temp);
        var operand = EmitExpression(operation.Operand, context, cancellationToken);
        context.CurrentBlock.AddInstruction(new MirUnaryInstruction(place, mirOp, operand));
        return new MirReadValue(place);
    }

    private MirReadValue EmitUnaryIncrementDecrement(
        BoundUnaryOperation operation,
        UnaryResultKind resultKind,
        UnaryArithmeticKind arithmeticKind,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(operation.Operand, context, cancellationToken);
        var valueType = operation.Operand.Type;

        var temp = context.CreateTemp(valueType, "old");
        var templaPlace = new MirLocalPlace(temp.Id, temp.Type);
        var oldValue = new MirReadValue(templaPlace);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(templaPlace, new MirReadValue(place))
        );

        var one = CreateUnitConstant(operation.Operand.Type, valueType);
        var binaryOp = arithmeticKind switch
        {
            UnaryArithmeticKind.Increment => MirBinaryOp.Addition,
            UnaryArithmeticKind.Decrement => MirBinaryOp.Subtraction,
            _ => throw new ArgumentOutOfRangeException(nameof(arithmeticKind)),
        };
        var result = context.CreateTemp(valueType, "result");
        var resultPlace = new MirLocalPlace(result);
        context.CurrentBlock.AddInstruction(
            new MirBinaryInstruction(resultPlace, binaryOp, oldValue, one)
        );
        var newValue = new MirReadValue(resultPlace);

        context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, newValue));

        return resultKind switch
        {
            UnaryResultKind.Prefix => newValue,
            UnaryResultKind.Postfix => oldValue,
            _ => throw new ArgumentOutOfRangeException(nameof(resultKind)),
        };
    }

    private MirReadValue EmitOperation(
        BoundBinaryOperation operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var left = EmitExpression(operation.Left, context, cancellationToken);
        var type = operation.Type;
        return EmitBinaryOperation(
            operation.Operation,
            type,
            left,
            operation.Right,
            context,
            cancellationToken
        );
    }

    private MirReadValue EmitBinaryOperation(
        BinaryOperation operation,
        TypeSymbol type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        return operation switch
        {
            BinaryOperation.LogicalAnd => EmitLogicalOperation(
                LogicalOperation.And,
                type,
                left,
                right,
                context,
                cancellationToken
            ),
            BinaryOperation.LogicalOr => EmitLogicalOperation(
                LogicalOperation.Or,
                type,
                left,
                right,
                context,
                cancellationToken
            ),
            _ => EmitSimpleBinaryOperation(
                operation,
                type,
                left,
                right,
                context,
                cancellationToken
            ),
        };
    }

    private MirReadValue EmitLogicalOperation(
        LogicalOperation operation,
        TypeSymbol type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var temp = context.CreateTemp(type);
        var place = new MirLocalPlace(temp);
        var evalNext = context.AddBlock("eval.right");
        var evalSkip = context.AddDetachedBlock("eval.skip");
        var evalMerge = context.AddDetachedBlock("eval.merge");

        var terminator = operation switch
        {
            LogicalOperation.And => new MirBranchTerminator(left, evalNext.Id, evalSkip.Id),
            LogicalOperation.Or => new MirBranchTerminator(left, evalSkip.Id, evalNext.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
        context.CurrentBlock.SetTerminator(terminator);

        context.SetCurrentBlock(evalNext);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(place, EmitExpression(right, context, cancellationToken))
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(evalMerge.Id));

        context.SetCurrentBlock(evalSkip);
        context.AddBlock(evalSkip);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(place, CreateBoolConstant(operation == LogicalOperation.Or))
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(evalMerge.Id));

        context.SetCurrentBlock(evalMerge);
        context.AddBlock(evalMerge);
        return new MirReadValue(place);
    }

    private MirReadValue EmitSimpleBinaryOperation(
        BinaryOperation operation,
        TypeSymbol type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var rightValue = EmitExpression(right, context, cancellationToken);
        var temp = context.CreateTemp(type);
        var place = new MirLocalPlace(temp.Id, temp.Type);
        context.CurrentBlock.AddInstruction(
            new MirBinaryInstruction(place, operation.ToMirBinaryOperation(), left, rightValue)
        );
        return new MirReadValue(place);
    }

    private MirNullValue EmitAssignment(
        BoundAssignmentOperation operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(operation.Left, context, cancellationToken);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            context.CurrentBlock.AddInstruction(
                new MirAssignInstruction(
                    place,
                    EmitExpression(operation.Right, context, cancellationToken)
                )
            );
        }
        else
        {
            var binaryOperation = operation.Operation.ToBinaryOperation();
            var loadPlace = new MirReadValue(place);
            var binaryValue = EmitBinaryOperation(
                binaryOperation,
                place.Type,
                loadPlace,
                operation.Right,
                context,
                cancellationToken
            );
            context.CurrentBlock.AddInstruction(new MirAssignInstruction(place, binaryValue));
        }

        return _nullValue;
    }

    private MirReadValue EmitConditional(
        BoundConditional operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var type = operation.Type;
        var temp = context.CreateTemp(type, "result");
        var place = new MirLocalPlace(temp.Id, temp.Type);

        var thenBlock = context.AddBlock("cond.then");
        var elseBlock = context.AddDetachedBlock("cond.else");
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        var condition = EmitExpression(operation.Condition, context, cancellationToken);
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, thenBlock.Id, elseBlock.Id)
        );

        context.SetCurrentBlock(thenBlock);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(
                place,
                EmitExpression(operation.WhenTrue, context, cancellationToken)
            )
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));

        context.AddBlock(elseBlock);
        context.SetCurrentBlock(elseBlock);
        context.CurrentBlock.AddInstruction(
            new MirAssignInstruction(
                place,
                EmitExpression(operation.WhenFalse, context, cancellationToken)
            )
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));

        context.AddBlock(mergeBlock);
        context.SetCurrentBlock(mergeBlock);
        return new MirReadValue(place);
    }

    private MirValue EmitCall(
        BoundInvocation call,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        MirLocalPlace? place;
        if (!call.Function.ReturnsVoid)
        {
            var returnType = call.Function.ReturnType;
            var local = context.CreateTemp(returnType, "result");
            place = new MirLocalPlace(local.Id, local.Type);
        }
        else
        {
            place = null;
        }

        context.CurrentBlock.AddInstruction(
            new MirCallInstruction(
                place,
                call.Function,
                EmitExpressionList(call.Arguments, context, cancellationToken)
            )
        );
        return place is not null ? new MirReadValue(place) : _nullValue;
    }

    private ImmutableArray<MirValue> EmitExpressionList(
        ImmutableArray<BoundExpression> arguments,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        return [.. arguments.Select(arg => EmitExpression(arg, context, cancellationToken))];
    }

    private MirReadValue EmitConversion(
        BoundConversion conversion,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var value = EmitExpression(conversion.Operand, context, cancellationToken);
        var target = conversion.Type;
        var temp = context.CreateTemp(target);
        var place = new MirLocalPlace(temp);
        context.CurrentBlock.AddInstruction(
            new MirConvertInstruction(place, conversion.Conversion, value)
        );
        return new MirReadValue(place);
    }

    private MirAddressOfValue EmitAddressOf(
        BoundAddressOf operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(operation.Operand, context, cancellationToken);
        return new MirAddressOfValue(place);
    }

    private MirReadValue EmitDereference(
        BoundDereference expression,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(expression, context, cancellationToken);
        return new MirReadValue(place);
    }

    private MirReadValue EmitIndex(
        BoundIndex index,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(index, context, cancellationToken);
        return new MirReadValue(place);
    }

    private MirPlace EmitPlace(
        BoundExpression expression,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        return expression switch
        {
            BoundVariableAccess access => EmitPlace(access, context),
            BoundParameterAccess access => EmitPlace(access, context),
            BoundDereference dereference => EmitPlace(dereference, context, cancellationToken),
            BoundIndex index => EmitPlace(index, context, cancellationToken),
            _ => throw new InvalidOperationException(
                $"Cannot emit place for expression of type {expression.GetType()}"
            ),
        };
    }

    private static MirPlace EmitPlace(BoundVariableAccess access, MirEmissionContext context)
    {
        if (context.TryGetLocal(access.Symbol) is { } local)
        {
            return new MirLocalPlace(local.Id, local.Type);
        }

        return new MirGlobalPlace(access.Symbol);
    }

    private static MirLocalPlace EmitPlace(BoundParameterAccess access, MirEmissionContext context)
    {
        var local = context.GetLocal(access.Symbol);
        return new MirLocalPlace(local.Id, local.Type);
    }

    private MirDerefPlace EmitPlace(
        BoundDereference dereference,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var pointer = EmitExpression(dereference.Operand, context, cancellationToken);
        var type = dereference.Type;
        return new MirDerefPlace(pointer, type);
    }

    private MirIndexPlace EmitPlace(
        BoundIndex indexer,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var operand = EmitPlace(indexer.Operand, context, cancellationToken);
        var index = EmitExpression(indexer.Index, context, cancellationToken);
        return new MirIndexPlace(operand, index, indexer.Type);
    }

    private MirConstantValue CreateBoolConstant(bool value)
    {
        return value ? _trueValue : _falseValue;
    }

    private static MirConstantValue CreateUnitConstant(TypeSymbol sourceType, TypeSymbol targetType)
    {
        return sourceType.SpecialType switch
        {
            SpecialType.I8 => new MirConstantValue(ConstantValue.I8(1), targetType),
            SpecialType.I16 => new MirConstantValue(ConstantValue.I16(1), targetType),
            SpecialType.I32 => new MirConstantValue(ConstantValue.I32(1), targetType),
            SpecialType.I64 => new MirConstantValue(ConstantValue.I64(1), targetType),
            SpecialType.I128 => new MirConstantValue(ConstantValue.I128(1), targetType),

            SpecialType.ISize => new MirConstantValue(ConstantValue.ISize(1), targetType),

            SpecialType.U8 => new MirConstantValue(ConstantValue.U8(1), targetType),
            SpecialType.U16 => new MirConstantValue(ConstantValue.U16(1), targetType),
            SpecialType.U32 => new MirConstantValue(ConstantValue.U32(1), targetType),
            SpecialType.U64 => new MirConstantValue(ConstantValue.U64(1), targetType),
            SpecialType.U128 => new MirConstantValue(ConstantValue.U128(1), targetType),

            SpecialType.USize => new MirConstantValue(ConstantValue.USize(1), targetType),

            SpecialType.F32 => new MirConstantValue(ConstantValue.F32(1), targetType),
            SpecialType.F64 => new MirConstantValue(ConstantValue.F64(1), targetType),

            _ => throw new InvalidOperationException(
                "Increment/decrement requires a numeric operand."
            ),
        };
    }
}
