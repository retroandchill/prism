// @file ControlFlowGraphBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class FunctionAnalysisBuilder
{
    private readonly FunctionSymbol _function;
    private readonly BindingContext _bindingContext;

    private readonly List<LoopContext> _loopStack = [];

    private readonly ImmutableHashSet<Symbol>.Builder _addressedLocals =
        ImmutableHashSet.CreateBuilder<Symbol>(ReferenceEqualityComparer.Instance);

    private readonly record struct LoopContext(LabelSymbol Label, bool HasReachableBreak = false);

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

    private FunctionAnalysisBuilder(FunctionSymbol function, BindingContext bindingContext)
    {
        _function = function;
        _bindingContext = bindingContext;
    }

    public static FunctionBodyAnalysis Build(
        FunctionSymbol function,
        BoundStatement body,
        BindingContext bindingContext
    )
    {
        var builder = new FunctionAnalysisBuilder(function, bindingContext);
        return builder.BuildCore(body);
    }

    private FunctionBodyAnalysis BuildCore(BoundStatement statement)
    {
        var state = new FlowState(new AnalysisState(), true);

        state = VisitStatement(statement, state);

        if (!_function.ReturnsVoid && state.IsReachable)
        {
            _bindingContext.ReportDiagnostic(
                Diagnostic.AllPathsMustReturnValue(statement.Syntax.Location)
            );
        }

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
            if (!state.IsReachable)
            {
                _bindingContext.ReportDiagnostic(
                    Diagnostic.UnreachableCode(statement.Syntax.Location)
                );
                break;
            }

            state = VisitStatement(statement, state);
        }

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

        if (statement.Condition.ConstantValue is { Kind: ConstantKind.Bool } constant)
        {
            if (constant.AsBoolean())
            {
                state = VisitStatement(statement.ThenStatement, state);
                if (statement.ElseStatement is not null)
                {
                    _bindingContext.ReportDiagnostic(
                        Diagnostic.UnreachableCode(statement.ElseStatement.Syntax.Location)
                    );
                }
            }
            else
            {
                _bindingContext.ReportDiagnostic(
                    Diagnostic.UnreachableCode(statement.ThenStatement.Syntax.Location)
                );
                if (statement.ElseStatement is not null)
                {
                    state = VisitStatement(statement.ElseStatement, state);
                }
            }

            return state;
        }

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
        _loopStack.Add(new LoopContext(statement.Label));

        state = VisitExpression(statement.Condition, state);

        if (statement.Condition.ConstantValue is { Kind: ConstantKind.Bool } constant)
        {
            if (constant.AsBoolean())
            {
                state = VisitStatement(statement.Body, state);

                if (!_loopStack[^1].HasReachableBreak)
                {
                    state = state.AsUnreachable();
                }
            }
            else
            {
                _bindingContext.ReportDiagnostic(
                    Diagnostic.UnreachableCode(statement.Body.Syntax.Location)
                );
            }
        }
        else
        {
            var bodyState = VisitStatement(statement.Body, state);
            state = state.Merge(bodyState);
        }

        _loopStack.RemoveAt(_loopStack.Count - 1);
        return state;
    }

    private FlowState VisitLoop(BoundLoopStatement statement, FlowState state)
    {
        _loopStack.Add(new LoopContext(statement.Label));

        state = VisitStatement(statement.Body, state);

        if (!_loopStack[^1].HasReachableBreak)
        {
            state = state.AsUnreachable();
        }
        _loopStack.RemoveAt(_loopStack.Count - 1);
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

        _loopStack.Add(new LoopContext(statement.Label));

        if (statement.Condition is not null)
        {
            state = VisitExpression(statement.Condition, state);

            if (statement.Condition.ConstantValue is { Kind: ConstantKind.Bool } constant)
            {
                if (constant.AsBoolean())
                {
                    state = VisitStatement(statement.Body, state);

                    if (state.IsReachable)
                    {
                        state = statement.Incrementors.Aggregate(
                            state,
                            (current, incrementor) => VisitExpression(incrementor, current)
                        );
                    }

                    if (_loopStack[^1].HasReachableBreak)
                    {
                        state = state.AsUnreachable();
                    }
                }
                else
                {
                    _bindingContext.ReportDiagnostic(
                        Diagnostic.UnreachableCode(statement.Body.Syntax.Location)
                    );
                }
            }
            else
            {
                var bodyState = VisitStatement(statement.Body, state);

                if (bodyState.IsReachable)
                {
                    var incrementorState = statement.Incrementors.Aggregate(
                        bodyState,
                        (current, incrementor) => VisitExpression(incrementor, current)
                    );

                    bodyState = bodyState.Merge(incrementorState);
                }

                state = state.Merge(bodyState);
            }
        }
        else
        {
            state = VisitStatement(statement.Body, state);

            if (state.IsReachable)
            {
                state = statement.Incrementors.Aggregate(
                    state,
                    (current, incrementor) => VisitExpression(incrementor, current)
                );
            }
        }

        _loopStack.RemoveAt(_loopStack.Count - 1);
        return state;
    }

    private FlowState VisitBreak(BoundBreakStatement statement, FlowState state)
    {
        ref var target = ref LookupLoop(statement.Label);
        target = target with { HasReachableBreak = true };
        return state.AsUnreachable();
    }

    private FlowState VisitContinue(BoundContinueStatement statement, FlowState state)
    {
        // TODO: There's probably more we need to do here
        return state.AsUnreachable();
    }

    private ref LoopContext LookupLoop(LabelSymbol label)
    {
        foreach (ref var loop in CollectionsMarshal.AsSpan(_loopStack))
        {
            if (ReferenceEquals(loop.Label, label))
                return ref loop;
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
