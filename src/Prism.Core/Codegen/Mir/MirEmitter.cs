// @file MirEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
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

    private readonly MirVoidValue _voidValue = new(compilation.GetSpecialType(SpecialType.Void));

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
        var builder = new MirFunctionBuilder(symbol) { Location = body.Location };

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
        context.EnterScope();
        foreach (var statement in block.Statements)
        {
            EmitStatement(statement, context, cancellationToken);
        }

        context.ExitScope();
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
            new MirStoreInstruction(new MirLocalPlace(local.Id, local.Type), operand)
            {
                Location = declaration.Location,
            }
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
            context.EmitEarlyReturnExit();
            context.CurrentBlock.SetTerminator(
                new MirReturnTerminator(result) { Location = statement.Location }
            );
        }
        else
        {
            context.EmitEarlyReturnExit();
            context.CurrentBlock.SetTerminator(
                new MirReturnTerminator { Location = statement.Location }
            );
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
        var loopHead = context.AddBlock("loop.head");
        var loopBody = loop.Condition is not null ? context.AddDetachedBlock("loop.body") : null;
        var loopIncrement =
            loop.Incrementors.Length > 0 ? context.AddDetachedBlock("loop.increment") : null;
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(loop.Label, loopTail.Id, loopIncrement?.Id ?? loopHead.Id);

        context.EnterScope();
        if (loop.Variable is not null)
        {
            EmitLocal(loop.Variable, context, cancellationToken);
        }

        foreach (var initializer in loop.Initializers)
        {
            EmitExpression(initializer, context, cancellationToken);
        }

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
        context.ExitScope();
    }

    private static void EmitBreakStatement(
        BoundBreakStatement statement,
        MirEmissionContext context
    )
    {
        var (breakTarget, _, targetScope) = context.GetLoopTargets(statement.Label);
        context.EmitExitTo(targetScope);
        context.CurrentBlock.SetTerminator(
            new MirGotoTerminator(breakTarget) { Location = statement.Location }
        );
    }

    private static void EmitContinueStatement(
        BoundContinueStatement statement,
        MirEmissionContext context
    )
    {
        var (_, continueTarget, targetScope) = context.GetLoopTargets(statement.Label);
        context.EmitExitTo(targetScope);
        context.CurrentBlock.SetTerminator(
            new MirGotoTerminator(continueTarget) { Location = statement.Location }
        );
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
            BoundCollectionExpression collection => EmitCollection(
                collection,
                context,
                cancellationToken
            ),
            BoundBadExpression or BoundSpeculativeExpression => throw new InvalidOperationException(
                "Should only emit LLVM IR if the compilation is valid"
            ),
            BoundLiteral => throw new InvalidOperationException(
                "The constant folding should have grabbed this already"
            ),
            _ => throw new InvalidOperationException("We probably added a new expression type"),
        };
    }

    private static MirSsaValue EmitAccess(BoundVariableAccess access, MirEmissionContext context)
    {
        var result = context.CreateSsaValue(access.Type);
        MirPlace place = context.TryGetLocal(access.Symbol) is { } local
            ? new MirLocalPlace(local.Id, local.Type)
            : new MirGlobalPlace(access.Symbol);
        context.CurrentBlock.AddInstruction(new MirLoadInstruction(result, place));
        return result;
    }

    private static MirSsaValue EmitAccess(BoundParameterAccess access, MirEmissionContext context)
    {
        var result = context.CreateSsaValue(access.Type);
        var local = context.GetLocal(access.Symbol);
        var place = new MirLocalPlace(local.Id, local.Type);
        context.CurrentBlock.AddInstruction(new MirLoadInstruction(result, place));
        return result;
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

    private MirSsaValue EmitSimpleUnary(
        BoundUnaryOperation operation,
        MirUnaryOp mirOp,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var operand = EmitExpression(operation.Operand, context, cancellationToken);
        var value = context.CreateSsaValue(operation.Type);
        context.CurrentBlock.AddInstruction(
            new MirUnaryInstruction(value, mirOp, operand) { Location = operation.Location }
        );
        return value;
    }

    private MirSsaValue EmitUnaryIncrementDecrement(
        BoundUnaryOperation operation,
        UnaryResultKind resultKind,
        UnaryArithmeticKind arithmeticKind,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(operation.Operand, context, cancellationToken);
        var valueType = operation.Operand.Type;

        var oldValue = context.CreateSsaValue(valueType);
        context.CurrentBlock.AddInstruction(
            new MirLoadInstruction(oldValue, place) { Location = operation.Location }
        );

        var one = CreateUnitConstant(operation.Operand.Type, valueType);
        var binaryOp = arithmeticKind switch
        {
            UnaryArithmeticKind.Increment => MirBinaryOp.Addition,
            UnaryArithmeticKind.Decrement => MirBinaryOp.Subtraction,
            _ => throw new ArgumentOutOfRangeException(nameof(arithmeticKind)),
        };
        var newValue = context.CreateSsaValue(valueType);
        context.CurrentBlock.AddInstruction(
            new MirBinaryInstruction(newValue, binaryOp, oldValue, one)
            {
                Location = operation.Location,
            }
        );

        context.CurrentBlock.AddInstruction(
            new MirStoreInstruction(place, newValue) { Location = operation.Location }
        );

        return resultKind switch
        {
            UnaryResultKind.Prefix => newValue,
            UnaryResultKind.Postfix => oldValue,
            _ => throw new ArgumentOutOfRangeException(nameof(resultKind)),
        };
    }

    private MirSsaValue EmitOperation(
        BoundBinaryOperation operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var left = EmitExpression(operation.Left, context, cancellationToken);
        var type = operation.Type;
        return EmitBinaryOperation(
            operation.Operation,
            operation.Location,
            type,
            left,
            operation.Right,
            context,
            cancellationToken
        );
    }

    private MirSsaValue EmitBinaryOperation(
        BinaryOperation operation,
        SourceLocation location,
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
                location,
                type,
                left,
                right,
                context,
                cancellationToken
            ),
        };
    }

    private MirSsaValue EmitLogicalOperation(
        LogicalOperation operation,
        TypeSymbol type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
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
        var rightResult = EmitExpression(right, context, cancellationToken);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(evalMerge.Id));

        context.SetCurrentBlock(evalSkip);
        context.AddBlock(evalSkip);
        var skipResult = CreateBoolConstant(operation == LogicalOperation.Or);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(evalMerge.Id));

        context.SetCurrentBlock(evalMerge);
        context.AddBlock(evalMerge);
        var result = context.CreateSsaValue(type);
        context.CurrentBlock.AddInstruction(
            new MirPhiInstruction(
                result,
                [new PhiSource(rightResult, evalNext.Id), new PhiSource(skipResult, evalSkip.Id)]
            )
        );
        return result;
    }

    private MirSsaValue EmitSimpleBinaryOperation(
        BinaryOperation operation,
        SourceLocation location,
        TypeSymbol type,
        MirValue left,
        BoundExpression right,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var rightValue = EmitExpression(right, context, cancellationToken);
        return EmitBinaryInstructions(operation, location, type, left, rightValue, context);
    }

    private static MirSsaValue EmitBinaryInstructions(
        BinaryOperation operation,
        SourceLocation location,
        TypeSymbol type,
        MirValue left,
        MirValue rightValue,
        MirEmissionContext context
    )
    {
        if (operation is BinaryOperation.Equality or BinaryOperation.NotEquals)
        {
            var isEqualityCheck = operation == BinaryOperation.Equality;

            if (left.Type is NullableTypeSymbol && rightValue.Type is NullableTypeSymbol)
            {
                var nullCheckResult = context.CreateSsaValue(type);

                if (left is MirConstantValue { IsNull: true })
                {
                    context.CurrentBlock.AddInstruction(
                        new MirIsNotNullInstruction(nullCheckResult, rightValue)
                        {
                            Location = location,
                        }
                    );
                }
                else if (rightValue is MirConstantValue { IsNull: true })
                {
                    context.CurrentBlock.AddInstruction(
                        new MirIsNotNullInstruction(nullCheckResult, left) { Location = location }
                    );
                }

                if (isEqualityCheck)
                    return nullCheckResult;

                var invertedNullCheck = context.CreateSsaValue(type);
                context.CurrentBlock.AddInstruction(
                    new MirUnaryInstruction(
                        invertedNullCheck,
                        MirUnaryOp.LogicalNot,
                        nullCheckResult
                    )
                    {
                        Location = location,
                    }
                );
                return invertedNullCheck;
            }
        }

        var result = context.CreateSsaValue(type);
        context.CurrentBlock.AddInstruction(
            new MirBinaryInstruction(result, operation.ToMirBinaryOperation(), left, rightValue)
            {
                Location = location,
            }
        );
        return result;
    }

    private MirVoidValue EmitAssignment(
        BoundAssignmentOperation operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(operation.Left, context, cancellationToken);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            context.CurrentBlock.AddInstruction(
                new MirStoreInstruction(
                    place,
                    EmitExpression(operation.Right, context, cancellationToken)
                )
                {
                    Location = operation.Location,
                }
            );
        }
        else
        {
            var binaryOperation = operation.Operation.ToBinaryOperation();
            var loadPlace = context.CreateSsaValue(place.Type);
            context.CurrentBlock.AddInstruction(new MirLoadInstruction(loadPlace, place));
            var binaryValue = EmitBinaryOperation(
                binaryOperation,
                operation.Location,
                place.Type,
                loadPlace,
                operation.Right,
                context,
                cancellationToken
            );
            context.CurrentBlock.AddInstruction(new MirStoreInstruction(place, binaryValue));
        }

        return _voidValue;
    }

    private MirSsaValue EmitConditional(
        BoundConditional operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var type = operation.Type;

        var condition = EmitExpression(operation.Condition, context, cancellationToken);

        var thenBlock = context.AddBlock("cond.then");
        var elseBlock = context.AddDetachedBlock("cond.else");
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(condition, thenBlock.Id, elseBlock.Id)
        );

        context.SetCurrentBlock(thenBlock);
        var thenResult = EmitExpression(operation.WhenTrue, context, cancellationToken);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));

        context.AddBlock(elseBlock);
        context.SetCurrentBlock(elseBlock);
        var elseResult = EmitExpression(operation.WhenFalse, context, cancellationToken);
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(mergeBlock.Id));

        context.AddBlock(mergeBlock);
        context.SetCurrentBlock(mergeBlock);

        var result = context.CreateSsaValue(type);
        context.CurrentBlock.AddInstruction(
            new MirPhiInstruction(
                result,
                [new PhiSource(thenResult, thenBlock.Id), new PhiSource(elseResult, elseBlock.Id)]
            )
        );
        return result;
    }

    private MirValue EmitCall(
        BoundInvocation call,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        MirSsaValue? result;
        if (!call.Function.ReturnsVoid)
        {
            var returnType = call.Function.ReturnType;
            result = context.CreateSsaValue(returnType);
        }
        else
        {
            result = null;
        }

        context.CurrentBlock.AddInstruction(
            new MirCallInstruction(
                result,
                call.Function,
                EmitExpressionList(call.Arguments, context, cancellationToken)
            )
            {
                Location = call.Location,
            }
        );
        return result is not null ? result : _voidValue;
    }

    private ImmutableArray<MirValue> EmitExpressionList(
        ImmutableArray<BoundExpression> arguments,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        return [.. arguments.Select(arg => EmitExpression(arg, context, cancellationToken))];
    }

    private MirValue EmitConversion(
        BoundConversion conversion,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var value = EmitExpression(conversion.Operand, context, cancellationToken);
        var target = conversion.Type;
        return PerformConversionOperation(
            conversion.Conversion,
            target,
            value,
            conversion.Location,
            context
        );
    }

    private MirValue PerformConversionOperation(
        Conversion conversion,
        TypeSymbol type,
        MirValue value,
        SourceLocation location,
        MirEmissionContext context
    )
    {
        if (conversion.IsNullable)
        {
            if (value.Type is NullableTypeSymbol nullableSource)
            {
                if (type is NullableTypeSymbol nullableTarget)
                {
                    return EmitNullableToNullableConversion(
                        type,
                        value,
                        conversion,
                        nullableSource,
                        nullableTarget,
                        location,
                        context
                    );
                }

                return EmitNullableToNotNullConversion(
                    type,
                    value,
                    conversion,
                    nullableSource,
                    location,
                    context
                );
            }

            var nullableValue = context.CreateSsaValue(type);
            context.CurrentBlock.AddInstruction(
                new MirMakeNullableInstruction(nullableValue, value) { Location = location }
            );

            return nullableValue;
        }

        var convertedValue = context.CreateSsaValue(type);
        context.CurrentBlock.AddInstruction(
            new MirConvertInstruction(convertedValue, conversion, value) { Location = location }
        );
        return convertedValue;
    }

    private MirValue EmitNullableToNullableConversion(
        TypeSymbol type,
        MirValue value,
        Conversion conversion,
        NullableTypeSymbol sourceType,
        NullableTypeSymbol targetType,
        SourceLocation location,
        MirEmissionContext context
    )
    {
        if (sourceType.ElementType is ReferenceTypeSymbol)
        {
            Debug.Assert(targetType.ElementType is ReferenceTypeSymbol);
            var converted = context.CreateSsaValue(type);
            context.CurrentBlock.AddInstruction(
                new MirConvertInstruction(converted, conversion, value) { Location = location }
            );
            return converted;
        }

        var condNotNull = context.AddBlock("cond.not.null");
        var condNull = context.AddDetachedBlock("cond.null");
        var condMerge = context.AddDetachedBlock("cond.merge");

        var nullableState = context.CreateSsaValue(compilation.GetSpecialType(SpecialType.Bool));
        context.CurrentBlock.AddInstruction(new MirIsNotNullInstruction(nullableState, value));
        context.CurrentBlock.SetTerminator(
            new MirBranchTerminator(nullableState, condNotNull.Id, condNull.Id)
        );

        context.SetCurrentBlock(condNotNull);

        var payload = context.CreateSsaValue(sourceType.ElementType);
        context.CurrentBlock.AddInstruction(new MirGetNullablePayloadInstruction(payload, value));

        MirValue payloadValue = payload;
        var underlyingConversions = conversion.UnderlyingConversions;
        if (underlyingConversions.Length > 0)
        {
            // Nullable conversions should only have one underlying conversion
            Debug.Assert(underlyingConversions.Length == 1);
            payloadValue = PerformConversionOperation(
                underlyingConversions[0],
                targetType.ElementType,
                payload,
                location,
                context
            );
        }

        var notNullValue = context.CreateSsaValue(targetType.ElementType);
        context.CurrentBlock.AddInstruction(
            new MirMakeNullableInstruction(notNullValue, payloadValue) { Location = location }
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(condMerge.Id));

        context.AddBlock(condNull);
        context.SetCurrentBlock(condNull);
        var nullValue = context.CreateSsaValue(targetType.ElementType);
        context.CurrentBlock.AddInstruction(
            new MirMakeNullableInstruction(nullValue, null) { Location = location }
        );
        context.CurrentBlock.SetTerminator(new MirGotoTerminator(condMerge.Id));

        context.AddBlock(condMerge);
        context.SetCurrentBlock(condMerge);
        var phi = context.CreateSsaValue(targetType.ElementType);
        context.CurrentBlock.AddInstruction(
            new MirPhiInstruction(
                phi,
                [new PhiSource(notNullValue, condNotNull.Id), new PhiSource(nullValue, condNull.Id)]
            )
        );
        return phi;
    }

    private MirValue EmitNullableToNotNullConversion(
        TypeSymbol type,
        MirValue value,
        Conversion conversion,
        NullableTypeSymbol sourceType,
        SourceLocation location,
        MirEmissionContext context
    )
    {
        if (sourceType.ElementType is ReferenceTypeSymbol)
        {
            Debug.Assert(type is ReferenceTypeSymbol);
            var payloadValue = context.CreateSsaValue(type);
            context.CurrentBlock.AddInstruction(
                new MirConvertInstruction(payloadValue, conversion, value) { Location = location }
            );
            return payloadValue;
        }

        var payload = context.CreateSsaValue(sourceType.ElementType);
        context.CurrentBlock.AddInstruction(new MirGetNullablePayloadInstruction(payload, value));

        var underlyingConversions = conversion.UnderlyingConversions;
        if (underlyingConversions.Length <= 0)
            return payload;

        // Nullable conversions should only have one underlying conversion
        Debug.Assert(underlyingConversions.Length == 1);
        return PerformConversionOperation(
            underlyingConversions[0],
            type,
            payload,
            location,
            context
        );
    }

    private MirAddressOfValue EmitAddressOf(
        BoundAddressOf operation,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        // If the operand is not directly addressable, then we're probably materializing a temporary
        MirPlace place;
        if (!operation.Operand.IsAddressable)
        {
            var value = EmitExpression(operation.Operand, context, cancellationToken);
            var local = context.CreateTemp(operation.Operand.Type);
            place = new MirLocalPlace(local);
            context.CurrentBlock.AddInstruction(new MirStoreInstruction(place, value));
        }
        else
        {
            place = EmitPlace(operation.Operand, context, cancellationToken);
        }
        return new MirAddressOfValue(place, operation.Type);
    }

    private MirSsaValue EmitDereference(
        BoundDereference expression,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(expression, context, cancellationToken);
        var result = context.CreateSsaValue(expression.Type);
        context.CurrentBlock.AddInstruction(new MirLoadInstruction(result, place));
        return result;
    }

    private MirSsaValue EmitIndex(
        BoundIndex index,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(index, context, cancellationToken);
        var result = context.CreateSsaValue(index.Type);
        context.CurrentBlock.AddInstruction(new MirLoadInstruction(result, place));
        return result;
    }

    private MirSsaValue EmitCollection(
        BoundCollectionExpression collection,
        MirEmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var local = context.CreateTemp(collection.Type);
        var place = new MirLocalPlace(local);
        foreach (var (i, value) in collection.Expressions.AsValueEnumerable().Index())
        {
            var indexConstant = new MirConstantValue(
                ConstantValue.USize((ulong)i),
                compilation.GetSpecialType(SpecialType.USize)
            );

            var index = new MirIndexPlace(place, indexConstant, collection.ElementType);
            context.CurrentBlock.AddInstruction(
                new MirStoreInstruction(index, EmitExpression(value, context, cancellationToken))
            );
        }

        var result = context.CreateSsaValue(collection.Type);
        context.CurrentBlock.AddInstruction(new MirLoadInstruction(result, place));
        return result;
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
