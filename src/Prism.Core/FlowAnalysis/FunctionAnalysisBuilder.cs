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

        public FlowState Merge(FlowState other)
        {
            // If either side is reachable, then there is a path to keep going
            return new FlowState(
                State: State.Merge(other.State),
                IsReachable: IsReachable || other.IsReachable
            );
        }
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
        return statement switch
        {
            BoundBlock block => VisitBlock(block, state),
            BoundVariableDeclaration declaration => VisitExpression(declaration.Initializer, state),
            BoundExpressionStatement expressionStatement => VisitExpression(
                expressionStatement.Expression,
                state
            ),
            BoundReturnStatement returnStatement => VisitReturn(returnStatement, state),
            BoundIfStatement ifStatement => VisitIf(ifStatement, state),
            BoundWhileStatement whileStatement => VisitWhile(whileStatement, state),
            BoundLoopStatement loopStatement => VisitLoop(loopStatement, state),
            BoundForStatement forStatement => VisitFor(forStatement, state),
            BoundBreakStatement breakStatement => VisitBreak(breakStatement, state),
            BoundContinueStatement continueStatement => VisitContinue(continueStatement, state),
            _ => throw new ArgumentOutOfRangeException(nameof(statement)),
        };
    }

    private FlowState VisitBlock(BoundBlock block, FlowState state)
    {
        foreach (var statement in block.Statements)
        {
            state = VisitStatement(statement, state);
            if (!state.IsReachable)
                break;
        }

        // TODO: We're going to copy Java and make unreachable code an explicit error
        return state;
    }

    private FlowState VisitReturn(BoundReturnStatement statement, FlowState state)
    {
        state = VisitExpression(statement.Expression, state);
        return state.AsUnreachable();
    }

    private FlowState VisitIf(BoundIfStatement statement, FlowState state)
    {
        state = VisitExpression(statement.Condition, state);

        var thenState = VisitStatement(statement.ThenStatement, state);

        if (statement.ElseStatement is null)
        {
            return thenState.Merge(state);
        }

        var elseState = VisitStatement(statement.ElseStatement, state);
        return thenState.Merge(elseState);
    }

    private FlowState VisitWhile(BoundWhileStatement statement, FlowState state)
    {
        _loopStack.Push(new LoopContext(statement.Label));

        state = VisitExpression(statement.Condition, state);

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
            state = VisitExpression(statement.Variable.Initializer, state);
        }

        state = statement.Initializers.Aggregate(
            state,
            (current, initializer) => VisitExpression(initializer, current)
        );

        _loopStack.Push(new LoopContext(statement.Label));

        if (statement.Condition is not null)
        {
            state = VisitExpression(statement.Condition, state);
        }

        state = VisitStatement(statement.Body, state);

        if (state.IsReachable)
        {
            state = statement.Incrementors.Aggregate(
                state,
                (current, incrementor) => VisitExpression(incrementor, current)
            );
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

    private FlowState VisitExpression(BoundExpression? expression, FlowState state)
    {
        while (true)
        {
            if (expression is null)
                return state;

            switch (expression)
            {
                case BoundAddressOf addressOf:
                    if (TryGetAddressedLocal(addressOf.Operand) is { } local)
                    {
                        _addressedLocals.Add(local);
                    }

                    expression = addressOf.Operand;
                    continue;

                case BoundUnaryOperation unary:
                    expression = unary.Operand;
                    continue;

                case BoundBinaryOperation binary:
                    state = VisitExpression(binary.Left, state);
                    expression = binary.Right;
                    continue;

                case BoundAssignmentOperation assignment:
                    state = VisitExpression(assignment.Left, state);
                    expression = assignment.Right;
                    continue;

                case BoundConditional conditional:
                    state = VisitExpression(conditional.Condition, state);
                    state = VisitExpression(conditional.WhenTrue, state);
                    state = VisitExpression(conditional.WhenFalse, state);
                    break;

                case BoundInvocation invocation:
                    state = invocation.Arguments.Aggregate(
                        state,
                        (current, argument) => VisitExpression(argument, current)
                    );

                    return state;

                case BoundConversion conversion:
                    expression = conversion.Operand;
                    continue;

                case BoundDereference dereference:
                    expression = dereference.Operand;
                    continue;

                case BoundIndex index:
                    state = VisitExpression(index.Operand, state);
                    expression = index.Index;
                    continue;
            }

            return state;
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
