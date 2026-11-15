// @file Emitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Compiler;
using Prism.Compiler.BoundTree;
using Prism.Compiler.Mappers;
using Prism.Compiler.Semantic;
using Prism.Configuration;
using Prism.Constants;
using Prism.Mir;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols;
using FloatKind = Prism.Constants.FloatKind;

namespace Prism.Codegen.Mir;

internal sealed class Emitter(Compilation compilation) : IEmitter
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

    public Function Emit(FunctionSymbol symbol, CancellationToken cancellationToken = default)
    {
        var boundBody = compilation.GetBoundBody(symbol, cancellationToken);

        var builder = new FunctionBuilder(symbol);
        var context = new EmissionContext(builder);

        if (boundBody.Body is not null)
        {
            EmitStatement(boundBody.Body, context, cancellationToken);

            if (!context.CurrentBlock.IsTerminated && symbol.ReturnsVoid)
            {
                context.CurrentBlock.SetReturnTerminator();
            }
        }
        else
        {
            context.CurrentBlock.SetUnreachable();
        }

        return builder.Build();
    }

    private void EmitStatement(
        BoundStatement statement,
        EmissionContext context,
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
        EmissionContext context,
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
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var (_, place) = context.BindLocal(declaration.Variable);
        if (declaration.Initializer is null)
            return;

        var lifetime = context.CreateExpressionLifetime();
        EmitExpression(place, declaration.Initializer, lifetime, context, cancellationToken);
        lifetime.EmitCleanup();
    }

    private void EmitExpressionStatement(
        BoundExpressionStatement statement,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var lifetime = context.CreateExpressionLifetime();
        _ = EmitExpression(statement.Expression, lifetime, context, cancellationToken);
        lifetime.EmitCleanup();
    }

    private void EmitReturn(
        BoundReturnStatement statement,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        if (statement.Expression is not null)
        {
            Debug.Assert(context.ReturnPlace is not null);
            var lifetime = context.CreateExpressionLifetime();
            EmitExpression(
                context.ReturnPlace,
                statement.Expression,
                lifetime,
                context,
                cancellationToken
            );
            lifetime.EmitCleanup();
        }

        context.EmitEarlyReturnExit();
        context.CurrentBlock.SetReturnTerminator();
    }

    private void EmitIfStatement(
        BoundIfStatement statement,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var thenBlock = context.AddBlock("then.cond");
        var elseBlock = context.AddDetachedBlock("else.cond");
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        var (conditionLifetime, condition) = EmitConditionCheck(
            statement.Condition,
            context,
            cancellationToken
        );
        context.CurrentBlock.SetBranchTerminator(condition, thenBlock, elseBlock);

        var thenFallsThrough = false;
        context.CurrentBlock = thenBlock;
        conditionLifetime.EmitCleanup();
        EmitStatement(statement.ThenStatement, context, cancellationToken);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetGotoTerminator(mergeBlock);
            thenFallsThrough = true;
        }

        var elseFallsThrough = false;
        context.AddBlock(elseBlock);
        context.CurrentBlock = elseBlock;
        conditionLifetime.EmitCleanup();
        if (statement.ElseStatement is not null)
        {
            EmitStatement(statement.ElseStatement, context, cancellationToken);
        }

        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetGotoTerminator(mergeBlock);
            elseFallsThrough = true;
        }

        if (!thenFallsThrough && !elseFallsThrough && statement.ElseStatement is not null)
            return;

        context.AddBlock(mergeBlock);
        context.CurrentBlock = mergeBlock;
    }

    private (ExpressionLifetime, Operand) EmitConditionCheck(
        BoundExpression condition,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        Debug.Assert(condition.Type.SpecialType == SpecialType.Bool);
        var conditionLifetime = context.CreateExpressionLifetime();
        var (_, place) = context.CreateTemporary(condition.Type, conditionLifetime);
        var lifetime = context.CreateExpressionLifetime();
        EmitExpression(place, condition, lifetime, context, cancellationToken);
        return (conditionLifetime, new MoveOperand(place));
    }

    private void EmitWhileStatement(
        BoundWhileStatement statement,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var loopHead = context.AddBlock("loop.head");
        var loopBody = context.AddDetachedBlock("loop.body");
        var loopCondFalse = context.AddDetachedBlock("loop.cond.false");
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(statement.Label, loopTail, loopHead);

        context.CurrentBlock.SetGotoTerminator(loopHead);

        var (conditionLifetime, condition) = EmitConditionCheck(
            statement.Condition,
            context,
            cancellationToken
        );
        context.CurrentBlock.SetBranchTerminator(condition, loopBody, loopCondFalse);

        context.AddBlock(loopBody);
        context.CurrentBlock = loopBody;
        conditionLifetime.EmitCleanup();
        EmitStatement(statement.Body, context, cancellationToken);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetGotoTerminator(loopHead);
        }

        context.AddBlock(loopCondFalse);
        context.CurrentBlock = loopCondFalse;
        conditionLifetime.EmitCleanup();
        context.CurrentBlock.SetGotoTerminator(loopTail);

        context.AddBlock(loopTail);
        context.CurrentBlock = loopTail;
    }

    private void EmitLoopStatement(
        BoundLoopStatement statement,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var loopHead = context.AddBlock("loop.head");
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(statement.Label, loopTail, loopHead);

        context.CurrentBlock.SetGotoTerminator(loopHead);
        context.CurrentBlock = loopHead;
        EmitStatement(statement.Body, context, cancellationToken);
        if (!context.CurrentBlock.IsTerminated)
        {
            context.CurrentBlock.SetGotoTerminator(loopHead);
        }

        context.AddBlock(loopTail);
        context.CurrentBlock = loopTail;
    }

    private void EmitForLoop(
        BoundForStatement loop,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var loopHead = context.AddBlock("loop.head");
        var loopBody = loop.Condition is not null ? context.AddDetachedBlock("loop.body") : null;
        var loopCondFalse = loop.Condition is not null
            ? context.AddDetachedBlock("loop.cond.false")
            : null;
        var loopIncrement =
            loop.Incrementors.Length > 0 ? context.AddDetachedBlock("loop.increment") : null;
        var loopTail = context.AddDetachedBlock("loop.tail");
        context.BindLoop(loop.Label, loopTail, loopIncrement ?? loopHead);

        context.EnterScope();
        if (loop.Variable is not null)
        {
            EmitLocal(loop.Variable, context, cancellationToken);
        }

        foreach (var initializer in loop.Initializers)
        {
            var lifetime = context.CreateExpressionLifetime();
            EmitExpression(initializer, lifetime, context, cancellationToken);
            lifetime.EmitCleanup();
        }

        context.CurrentBlock.SetGotoTerminator(loopHead);
        context.CurrentBlock = loopHead;
        if (loopBody is not null)
        {
            Debug.Assert(loopCondFalse is not null);
            Debug.Assert(loop.Condition is not null);
            var (conditionLifetime, condition) = EmitConditionCheck(
                loop.Condition,
                context,
                cancellationToken
            );
            context.CurrentBlock.SetBranchTerminator(condition, loopBody, loopCondFalse);

            // We do it this way so the displayed order is body, then cond.false, but we want to emit the cleanup here first
            context.AddBlock(loopBody);

            context.AddBlock(loopCondFalse);
            context.CurrentBlock = loopCondFalse;
            conditionLifetime.EmitCleanup();
            context.CurrentBlock.SetGotoTerminator(loopTail);

            context.CurrentBlock = loopBody;
        }

        EmitStatement(loop.Body, context, cancellationToken);

        if (!context.CurrentBlock.IsTerminated)
        {
            if (loopIncrement is not null)
            {
                context.CurrentBlock.SetGotoTerminator(loopIncrement);

                context.AddBlock(loopIncrement);
                context.CurrentBlock = loopIncrement;
                foreach (var incrementor in loop.Incrementors)
                {
                    var lifetime = context.CreateExpressionLifetime();
                    EmitExpression(incrementor, lifetime, context, cancellationToken);
                    lifetime.EmitCleanup();
                }
            }

            context.CurrentBlock.SetGotoTerminator(loopHead);
        }

        context.AddBlock(loopTail);
        context.CurrentBlock = loopTail;
        context.ExitScope();
    }

    private static void EmitBreakStatement(BoundBreakStatement statement, EmissionContext context)
    {
        var (breakTarget, _, targetScope) = context.GetLoopTargets(statement.Label);
        context.EmitExitTo(targetScope);
        context.CurrentBlock.SetGotoTerminator(breakTarget);
    }

    private static void EmitContinueStatement(
        BoundContinueStatement statement,
        EmissionContext context
    )
    {
        var (_, continueTarget, targetScope) = context.GetLoopTargets(statement.Label);
        context.EmitExitTo(targetScope);
        context.CurrentBlock.SetGotoTerminator(continueTarget);
    }

    private Operand EmitExpression(
        BoundExpression expression,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (expression.ConstantValue is { } constantValue)
            {
                return ConstantOperand.Create(in constantValue);
            }

            if (
                expression is BoundUnaryOperation
                {
                    Operation: UnaryOperation.Identity,
                    Operand: var operand
                }
            )
            {
                expression = operand;
                continue;
            }

            switch (expression)
            {
                case BoundVariableAccess access:
                    return EmitAccess(access, lifetime, context, cancellationToken);
                case BoundParameterAccess access:
                    return EmitAccess(access, context);
                case BoundThisExpression:
                    return EmitThis(context);
                case BoundAssignmentOperation assignment:
                    return EmitAssignment(assignment, lifetime, context, cancellationToken);
            }

            Place place;
            if (!expression.Type.IsVoid)
            {
                (_, place) = context.CreateTemporary(expression.Type, lifetime);
            }
            else
            {
                place = default;
            }

            EmitExpression(place, expression, lifetime, context, cancellationToken);
            return new MoveOperand(place);
        }
    }

    private void EmitExpression(
        Place destination,
        BoundExpression expression,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (expression.ConstantValue is { } constantValue)
        {
            context.CurrentBlock.AddAssignment(
                destination,
                new UseRValue(ConstantOperand.Create(in constantValue))
            );
            return;
        }

        switch (expression)
        {
            case BoundVariableAccess access:
                context.CurrentBlock.AddAssignment(
                    destination,
                    new UseRValue(EmitAccess(access, lifetime, context, cancellationToken))
                );
                break;
            case BoundParameterAccess access:
                context.CurrentBlock.AddAssignment(
                    destination,
                    new UseRValue(EmitAccess(access, context))
                );
                break;
            case BoundThisExpression:
                context.CurrentBlock.AddAssignment(destination, new UseRValue(EmitThis(context)));
                break;
            case BoundUnaryOperation unary:
                EmitOperation(destination, unary, lifetime, context, cancellationToken);
                break;
            case BoundBinaryOperation binary:
                EmitOperation(destination, binary, lifetime, context, cancellationToken);
                break;
            case BoundConditional conditional:
                EmitConditional(destination, conditional, lifetime, context, cancellationToken);
                break;
            case BoundInvocation invocation:
                break;
            case BoundConversion conversion:
                break;
            case BoundAddressOf addressOf:
                break;
            case BoundDereference dereference:
                break;
            case BoundIndex index:
                break;
            case BoundCollectionExpression collection:
                break;
            case BoundNewExpression newExpression:
                break;
            case BoundBadExpression or BoundSpeculativeExpression:
                throw new InvalidOperationException(
                    "Should only emit LLVM IR if the compilation is valid"
                );
            case BoundLiteral or BoundTypeSize:
                throw new InvalidOperationException(
                    "The constant folding should have grabbed this already"
                );
            case BoundAssignmentOperation:
                throw new InvalidOperationException("Assignments do not return values");
            default:
                throw new ArgumentOutOfRangeException(nameof(expression));
        }
    }

    private CopyOperand EmitAccess(
        BoundVariableAccess access,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        return new CopyOperand(EmitPlace(access, lifetime, context, cancellationToken));
    }

    private static CopyOperand EmitAccess(BoundParameterAccess access, EmissionContext context)
    {
        return new CopyOperand(EmitPlace(access, context));
    }

    private static CopyOperand EmitThis(EmissionContext context)
    {
        return new CopyOperand(EmitThisPlace(context));
    }

    private void EmitOperation(
        Place destination,
        BoundUnaryOperation operation,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        switch (operation.Operation)
        {
            case UnaryOperation.Identity:
                EmitExpression(
                    destination,
                    operation.Operand,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.Negation:
                EmitSimpleUnary(
                    destination,
                    UnaryOperator.Negation,
                    operation.Operand,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.LogicalNot:
                EmitSimpleUnary(
                    destination,
                    UnaryOperator.LogicalNot,
                    operation.Operand,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.BitwiseNot:
                EmitSimpleUnary(
                    destination,
                    UnaryOperator.BitwiseNot,
                    operation.Operand,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.PreIncrement:
                EmitUnaryIncrementDecrement(
                    destination,
                    operation.Operand,
                    UnaryResultKind.Prefix,
                    UnaryArithmeticKind.Increment,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.PreDecrement:
                EmitUnaryIncrementDecrement(
                    destination,
                    operation.Operand,
                    UnaryResultKind.Prefix,
                    UnaryArithmeticKind.Decrement,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.PostIncrement:
                EmitUnaryIncrementDecrement(
                    destination,
                    operation.Operand,
                    UnaryResultKind.Postfix,
                    UnaryArithmeticKind.Increment,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case UnaryOperation.PostDecrement:
                EmitUnaryIncrementDecrement(
                    destination,
                    operation.Operand,
                    UnaryResultKind.Postfix,
                    UnaryArithmeticKind.Decrement,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            default:
                throw new InvalidOperationException("Unknown unary operation");
        }
    }

    private void EmitSimpleUnary(
        Place destination,
        UnaryOperator operation,
        BoundExpression inner,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        context.CurrentBlock.AddAssignment(
            destination,
            new UnaryRValue(operation, EmitExpression(inner, lifetime, context, cancellationToken))
        );
    }

    private void EmitUnaryIncrementDecrement(
        Place place,
        BoundExpression inner,
        UnaryResultKind resultKind,
        UnaryArithmeticKind arithmeticKind,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var expressionPlace = EmitPlace(inner, lifetime, context, cancellationToken);
        var binaryOperand = arithmeticKind switch
        {
            UnaryArithmeticKind.Increment => BinaryOperator.Addition,
            UnaryArithmeticKind.Decrement => BinaryOperator.Subtraction,
            _ => throw new ArgumentOutOfRangeException(
                nameof(arithmeticKind),
                arithmeticKind,
                null
            ),
        };

        var copyOperand = new CopyOperand(expressionPlace);
        if (resultKind == UnaryResultKind.Postfix)
        {
            context.CurrentBlock.AddAssignment(place, new UseRValue(copyOperand));
        }

        Debug.Assert(inner.Type.SpecialType.IsNumeric);
        context.CurrentBlock.AddAssignment(
            expressionPlace,
            new BinaryRValue(binaryOperand, copyOperand, GetUnitConstant(inner.Type.SpecialType))
        );

        if (resultKind == UnaryResultKind.Postfix)
        {
            context.CurrentBlock.AddAssignment(place, new UseRValue(copyOperand));
        }
    }

    private void EmitOperation(
        Place destination,
        BoundBinaryOperation operation,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var left = EmitExpression(operation.Left, lifetime, context, cancellationToken);
        Debug.Assert(operation.Left == operation.Right);
        EmitBinaryOperation(
            destination,
            operation.Operation,
            operation.Left.Type,
            left,
            operation.Right,
            lifetime,
            context,
            cancellationToken
        );
    }

    private void EmitBinaryOperation(
        Place destination,
        BinaryOperation operation,
        TypeSymbol inputType,
        Operand left,
        BoundExpression right,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        switch (operation)
        {
            case BinaryOperation.LogicalAnd:
                EmitLogicalOperation(
                    destination,
                    LogicalOperation.And,
                    left,
                    right,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            case BinaryOperation.LogicalOr:
                EmitLogicalOperation(
                    destination,
                    LogicalOperation.Or,
                    left,
                    right,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
            default:
                EmitSimpleBinaryOperation(
                    destination,
                    operation,
                    inputType,
                    left,
                    right,
                    lifetime,
                    context,
                    cancellationToken
                );
                break;
        }
    }

    private void EmitLogicalOperation(
        Place destination,
        LogicalOperation operation,
        Operand left,
        BoundExpression right,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var evalNext = context.AddBlock("eval.right");
        var evalSkip = context.AddDetachedBlock("eval.skip");
        var evalMerge = context.AddDetachedBlock("eval.merge");

        switch (operation)
        {
            case LogicalOperation.And:
                context.CurrentBlock.SetBranchTerminator(left, evalNext, evalSkip);
                break;
            case LogicalOperation.Or:
                context.CurrentBlock.SetBranchTerminator(left, evalSkip, evalNext);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
        }

        context.CurrentBlock = evalNext;
        EmitExpression(destination, right, lifetime, context, cancellationToken);
        context.CurrentBlock.SetGotoTerminator(evalMerge);

        context.CurrentBlock = evalSkip;
        context.AddBlock(evalSkip);
        var skipResult = operation switch
        {
            LogicalOperation.And => ConstantOperand.False,
            LogicalOperation.Or => ConstantOperand.True,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
        context.CurrentBlock.AddAssignment(destination, new UseRValue(skipResult));
        context.CurrentBlock.SetGotoTerminator(evalMerge);

        context.CurrentBlock = evalMerge;
        context.AddBlock(evalMerge);
    }

    private void EmitSimpleBinaryOperation(
        Place destination,
        BinaryOperation operation,
        TypeSymbol inputType,
        Operand left,
        BoundExpression right,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var rightValue = EmitExpression(right, lifetime, context, cancellationToken);
        EmitBinaryStatement(destination, operation, inputType, left, rightValue, context);
    }

    private static void EmitBinaryStatement(
        Place destination,
        BinaryOperation operation,
        TypeSymbol inputType,
        Operand left,
        Operand right,
        EmissionContext context
    )
    {
        if (
            operation is BinaryOperation.Equality or BinaryOperation.NotEquals
            && inputType is NullableTypeSymbol
        )
        {
            var shouldBeNull = operation == BinaryOperation.Equality;
            if (left is ConstantOperand { IsNull: true })
            {
                context.CurrentBlock.AddAssignment(
                    destination,
                    shouldBeNull ? new IsNull(right) : new IsNotNull(right)
                );
                return;
            }

            if (right is ConstantOperand { IsNull: true })
            {
                context.CurrentBlock.AddAssignment(
                    destination,
                    shouldBeNull ? new IsNotNull(left) : new IsNull(left)
                );
                return;
            }
        }

        context.CurrentBlock.AddAssignment(
            destination,
            new BinaryRValue(operation.ToBinaryOperator(), left, right)
        );
    }

    private ConstantOperand EmitAssignment(
        BoundAssignmentOperation operation,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var place = EmitPlace(operation.Left, lifetime, context, cancellationToken);
        if (operation.Operation == AssignmentOperation.Simple)
        {
            EmitExpression(place, operation.Right, lifetime, context, cancellationToken);
        }
        else
        {
            var binaryOperation = operation.Operation.ToBinaryOperation();
            EmitBinaryOperation(
                place,
                binaryOperation,
                operation.Left.Type,
                new CopyOperand(place),
                operation.Right,
                lifetime,
                context,
                cancellationToken
            );
        }

        // This is just a placeholder value as assignments do not return a value
        return ConstantOperand.Null;
    }

    private void EmitConditional(
        Place destination,
        BoundConditional operation,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var condition = EmitExpression(operation.Condition, lifetime, context, cancellationToken);

        if (
            operation.WhenTrue.ConstantValue is { } trueConstant
            && operation.WhenFalse.ConstantValue is { } falseConstant
        )
        {
            context.CurrentBlock.AddAssignment(
                destination,
                new SelectRValue(
                    condition,
                    ConstantOperand.Create(in trueConstant),
                    ConstantOperand.Create(in falseConstant)
                )
            );
            return;
        }

        var thenBlock = context.AddBlock("cond.then");
        var elseBlock = context.AddDetachedBlock("cond.else");
        var mergeBlock = context.AddDetachedBlock("cond.merge");

        context.CurrentBlock.SetBranchTerminator(condition, thenBlock, elseBlock);

        context.CurrentBlock = thenBlock;
        var value = EmitExpression(operation.WhenTrue, lifetime, context, cancellationToken);
        context.CurrentBlock.AddAssignment(destination, new UseRValue(value));
        context.CurrentBlock.SetGotoTerminator(mergeBlock);

        context.CurrentBlock = elseBlock;
        context.AddBlock(elseBlock);
        value = EmitExpression(operation.WhenFalse, lifetime, context, cancellationToken);
        context.CurrentBlock.AddAssignment(destination, new UseRValue(value));
        context.CurrentBlock.SetGotoTerminator(mergeBlock);

        context.CurrentBlock = mergeBlock;
        context.AddBlock(mergeBlock);
    }

    private Place EmitPlace(
        BoundExpression expression,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var builder = new PlaceBuilder();
        EmitPlace(ref builder, expression, lifetime, context, cancellationToken);
        return builder.Build();
    }

    private void EmitPlace(
        ref PlaceBuilder builder,
        BoundExpression expression,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        switch (expression)
        {
            case BoundVariableAccess access:
                EmitPlace(ref builder, access, lifetime, context, cancellationToken);
                break;
            case BoundParameterAccess access:
                EmitPlace(ref builder, access, context);
                break;
            case BoundDereference dereference:
                EmitPlace(ref builder, dereference, lifetime, context, cancellationToken);
                break;
            case BoundIndex index:
                EmitPlace(ref builder, index, lifetime, context, cancellationToken);
                break;
            case BoundThisExpression:
                EmitThisPlace(ref builder, context);
                break;
            default:
            {
                var (_, place) = context.CreateTemporary(expression.Type, lifetime);
                EmitExpression(place, expression, lifetime, context, cancellationToken);
                builder.Base = place;
                break;
            }
        }
    }

    private Place EmitPlace(
        BoundVariableAccess access,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        var builder = new PlaceBuilder();
        EmitPlace(ref builder, access, lifetime, context, cancellationToken);
        return builder.Build();
    }

    private void EmitPlace(
        ref PlaceBuilder builder,
        BoundVariableAccess access,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        if (access.Owner is not null)
        {
            EmitPlace(ref builder, access.Owner, lifetime, context, cancellationToken);
            builder.AddProjection(new FieldProjection(access.Symbol));
        }
        else if (context.TryGetLocal(access.Symbol) is var (_, place))
        {
            builder.Base = place;
        }
        else
        {
            builder.Base = new GlobalPlace(access.Symbol);
        }
    }

    private static Place EmitPlace(BoundParameterAccess access, EmissionContext context)
    {
        return context.GetLocal(access.Symbol).Place;
    }

    private static void EmitPlace(
        ref PlaceBuilder builder,
        BoundParameterAccess access,
        EmissionContext context
    )
    {
        builder.Base = context.GetLocal(access.Symbol).Place;
    }

    private void EmitPlace(
        ref PlaceBuilder builder,
        BoundDereference dereference,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        EmitPlace(ref builder, dereference.Operand, lifetime, context, cancellationToken);
        builder.AddProjection(DereferenceProjection.Instance);
    }

    private void EmitPlace(
        ref PlaceBuilder builder,
        BoundIndex indexer,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        EmitPlace(ref builder, indexer.Operand, lifetime, context, cancellationToken);
        var index = EmitExpression(indexer.Index, lifetime, context, cancellationToken);
        builder.AddProjection(new IndexProjection(index));
    }

    private static Place EmitThisPlace(EmissionContext context)
    {
        Debug.Assert(context.ReceiverPlace is not null);
        return context.ReceiverPlace;
    }

    private static void EmitThisPlace(ref PlaceBuilder builder, EmissionContext context)
    {
        Debug.Assert(context.ReceiverPlace is not null);
        builder.Base = context.ReceiverPlace;
    }

    private ConstantOperand GetUnitConstant(SpecialType type)
    {
        return type switch
        {
            SpecialType.I8 => ConstantOperand.One(IntegerKind.I8),
            SpecialType.I16 => ConstantOperand.One(IntegerKind.I16),
            SpecialType.I32 => ConstantOperand.One(IntegerKind.I32),
            SpecialType.I64 => ConstantOperand.One(IntegerKind.I64),
            SpecialType.I128 => ConstantOperand.One(IntegerKind.I128),
            SpecialType.ISize => compilation.Settings.PointerWidth switch
            {
                PointerWidth.X32 => ConstantOperand.One(IntegerKind.I32),
                PointerWidth.X64 => ConstantOperand.One(IntegerKind.I64),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            SpecialType.U8 => ConstantOperand.One(IntegerKind.U8),
            SpecialType.U16 => ConstantOperand.One(IntegerKind.U16),
            SpecialType.U32 => ConstantOperand.One(IntegerKind.U32),
            SpecialType.U64 => ConstantOperand.One(IntegerKind.U64),
            SpecialType.U128 => ConstantOperand.One(IntegerKind.U128),
            SpecialType.USize => compilation.Settings.PointerWidth switch
            {
                PointerWidth.X32 => ConstantOperand.One(IntegerKind.U32),
                PointerWidth.X64 => ConstantOperand.One(IntegerKind.U64),
                _ => throw new InvalidOperationException("Invalid pointer width"),
            },
            SpecialType.F32 => ConstantOperand.One(FloatKind.F32),
            SpecialType.F64 => ConstantOperand.One(FloatKind.F64),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
