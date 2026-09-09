// @file ControlFlowGraphBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.BoundTree;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class FunctionAnalysisBuilder
{
    private readonly Stack<LoopContext> _loopStack = [];

    private readonly ImmutableHashSet<Symbol>.Builder _addressedLocals =
        ImmutableHashSet.CreateBuilder<Symbol>(ReferenceEqualityComparer.Instance);

    private readonly record struct LoopContext(LabelSymbol Label);

    private readonly record struct FlowState(AnalysisState State, bool IsReachable)
    {
        public FlowState AsUnreachable() => this with { IsReachable = false };
    }

    private FunctionAnalysisBuilder() { }

    public static FunctionBodyAnalysis Build(BoundStatement body)
    {
        var builder = new FunctionAnalysisBuilder();
        return builder.BuildCore(body);
    }

    private FunctionBodyAnalysis BuildCore(BoundStatement statement)
    {
        var state = new FlowState(new AnalysisState(), true);

        VisitStatement(statement, state);

        return new FunctionBodyAnalysis(_addressedLocals.ToImmutable());
    }

    private FlowState VisitStatement(BoundStatement statement, FlowState state)
    {
        switch (statement)
        {
            case BoundBlock block:
                return VisitBlock(block, state);
            case BoundVariableDeclaration declaration:
                VisitExpression(declaration.Initializer);
                return state;
            case BoundExpressionStatement expressionStatement:
                VisitExpression(expressionStatement.Expression);
                return state;
            case BoundReturnStatement returnStatement:
                return VisitReturn(returnStatement, state);
            case BoundIfStatement ifStatement:
                return VisitIf(ifStatement, state);
            case BoundWhileStatement whileStatement:
                return VisitWhile(whileStatement, state);
            case BoundLoopStatement loopStatement:
                return VisitLoop(loopStatement, state);
            case BoundForStatement forStatement:
                return VisitFor(forStatement, state);
            case BoundBreakStatement breakStatement:
                return VisitBreak(breakStatement, state);
            case BoundContinueStatement continueStatement:
                return VisitContinue(continueStatement, state);
            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private FlowState VisitBlock(BoundBlock block, FlowState state)
    {
        return block.Statements.Aggregate(
            state,
            (current, statement) => VisitStatement(statement, current)
        );
    }

    private FlowState VisitReturn(BoundReturnStatement statement, FlowState state)
    {
        VisitExpression(statement.Expression);
        return state.AsUnreachable();
    }

    private FlowState VisitIf(BoundIfStatement statement, FlowState state)
    {
        state = VisitStatement(statement.ThenStatement, state);

        if (statement.ElseStatement is not null)
        {
            state = VisitStatement(statement.ElseStatement, state);
        }

        return state;
    }

    private FlowState VisitWhile(BoundWhileStatement statement, FlowState state)
    {
        _loopStack.Push(new LoopContext(statement.Label));

        VisitExpression(statement.Condition);

        state = VisitStatement(statement.Body, state);

        _loopStack.Pop();
        return state;
    }

    private FlowState VisitLoop(BoundLoopStatement statement, FlowState state)
    {
        _loopStack.Push(new LoopContext(statement.Label));

        state = VisitStatement(statement.Body, state);

        _loopStack.Pop();
        return state;
    }

    private FlowState VisitFor(BoundForStatement statement, FlowState state)
    {
        if (statement.Variable is not null)
        {
            VisitExpression(statement.Variable.Initializer);
        }

        foreach (var initializer in statement.Initializers)
        {
            VisitExpression(initializer);
        }

        _loopStack.Push(new LoopContext(statement.Label));

        if (statement.Condition is not null)
        {
            VisitExpression(statement.Condition);
        }

        state = VisitStatement(statement.Body, state);

        if (state.IsReachable)
        {
            foreach (var incrementor in statement.Incrementors)
            {
                VisitExpression(incrementor);
            }
        }

        _loopStack.Pop();
        return state;
    }

    private FlowState VisitBreak(BoundBreakStatement statement, FlowState state)
    {
        var target = LookupLoop(statement.Label);
        return state.AsUnreachable();
    }

    private FlowState VisitContinue(BoundContinueStatement statement, FlowState state)
    {
        var target = LookupLoop(statement.Label);
        return state.AsUnreachable();
    }

    private LoopContext LookupLoop(LabelSymbol label)
    {
        foreach (var loop in _loopStack)
        {
            if (ReferenceEquals(loop.Label, label))
                return loop;
        }

        throw new InvalidOperationException("Loop label not found");
    }

    private void VisitExpression(BoundExpression? expression)
    {
        if (expression is null)
            return;

        switch (expression)
        {
            case BoundAddressOf addressOf:
                if (TryGetAddressedLocal(addressOf.Operand) is { } local)
                {
                    _addressedLocals.Add(local);
                }

                VisitExpression(addressOf.Operand);
                break;

            case BoundUnaryOperation unary:
                VisitExpression(unary.Operand);
                break;

            case BoundBinaryOperation binary:
                VisitExpression(binary.Left);
                VisitExpression(binary.Right);
                break;

            case BoundAssignmentOperation assignment:
                VisitExpression(assignment.Left);
                VisitExpression(assignment.Right);
                break;

            case BoundConditional conditional:
                VisitExpression(conditional.Condition);
                VisitExpression(conditional.WhenTrue);
                VisitExpression(conditional.WhenFalse);
                break;

            case BoundInvocation invocation:
                foreach (var argument in invocation.Arguments)
                {
                    VisitExpression(argument);
                }
                break;

            case BoundConversion conversion:
                VisitExpression(conversion.Operand);
                break;

            case BoundDereference dereference:
                VisitExpression(dereference.Operand);
                break;

            case BoundIndex index:
                VisitExpression(index.Operand);
                VisitExpression(index.Index);
                break;
        }
    }

    private static Symbol? TryGetAddressedLocal(BoundExpression expression)
    {
        return expression switch
        {
            BoundVariableAccess variableAccess => variableAccess.Symbol,
            BoundParameterAccess parameterAccess => parameterAccess.Symbol,
            _ => null,
        };
    }
}
