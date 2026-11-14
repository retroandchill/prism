// @file Emitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Compiler;
using Prism.Compiler.BoundTree;
using Prism.Mir;
using Prism.Semantic.Symbols;

namespace Prism.Codegen.Mir;

internal sealed class Emitter(Compilation compilation) : IEmitter
{
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
        throw new NotImplementedException();
    }

    private void EmitExpression(
        Place destination,
        BoundExpression expression,
        ExpressionLifetime lifetime,
        EmissionContext context,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
