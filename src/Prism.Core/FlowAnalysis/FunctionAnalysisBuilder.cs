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
using Prism.Core.Symbols.Synthesized;
using Prism.Core.Utils;

namespace Prism.Core.FlowAnalysis;

internal sealed class FunctionAnalysisBuilder
{
    [Flags]
    private enum Flags
    {
        None = 0,
        ConstructingArguments = 1 << 0,
    }

    private readonly ref struct FlagScope : IDisposable
    {
        private readonly FunctionAnalysisBuilder _builder;
        private readonly Flags _previousFlags;

        public FlagScope(FunctionAnalysisBuilder builder, Flags flags)
        {
            _builder = builder;
            _previousFlags = builder._flags;
            builder._flags |= flags;
        }

        public void Dispose()
        {
            _builder._flags = _previousFlags;
        }
    }

    private readonly FunctionSymbol _function;
    private readonly BindingContext _bindingContext;
    private readonly CancellationToken _cancellationToken;

    private readonly List<LoopContext> _loopStack = [];
    private readonly Dictionary<string, List<VariableSymbol>> _localsByName = new();
    private Flags _flags = Flags.None;

    private readonly ImmutableHashSet<Symbol>.Builder _addressedLocals =
        ImmutableHashSet.CreateBuilder<Symbol>(ReferenceEqualityComparer.Instance);

    private readonly record struct LoopContext(LabelSymbol Label, bool HasReachableBreak = false);

    private readonly record struct FlowState(AnalysisState State, bool IsReachable)
    {
        public FlowState AsUnreachable() => this with { IsReachable = false };

        public FlowState Merge(FlowState other)
        {
            if (!IsReachable)
                return other;

            if (!other.IsReachable)
                return this;

            return new FlowState(State: State.Merge(other.State), IsReachable: true);
        }
    }

    internal readonly record struct ConditionFlow(AnalysisState WhenTrue, AnalysisState WhenFalse);

    private FunctionAnalysisBuilder(
        FunctionSymbol function,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        _function = function;
        _bindingContext = bindingContext;
        _cancellationToken = cancellationToken;
    }

    public static FunctionBodyAnalysis Build(
        FunctionSymbol function,
        BoundStatement body,
        BindingContext bindingContext,
        CancellationToken cancellationToken
    )
    {
        var builder = new FunctionAnalysisBuilder(function, bindingContext, cancellationToken);
        return builder.BuildCore(body);
    }

    private FunctionBodyAnalysis BuildCore(BoundStatement statement)
    {
        var state = new FlowState(AnalysisState.Empty, true);

        state = VisitStatement(statement, state);

        _cancellationToken.ThrowIfCancellationRequested();

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
            BoundVariableDeclaration declaration => VisitLocal(declaration, state),
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
        _cancellationToken.ThrowIfCancellationRequested();

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

    private FlowState VisitLocal(BoundVariableDeclaration declaration, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var locals = _localsByName.GetOrAdd(declaration.Variable.Name, () => []);
        if (
            _function.Parameters.Any(x => x.Name == declaration.Variable.Name)
            || locals
                .Select(local => state.State.GetVariableInitialization(local))
                .Any(initialization => initialization != InitializationState.Undeclared)
        )
        {
            _bindingContext.ReportDiagnostic(
                Diagnostic.LocalNameShadowing(declaration.Location, declaration.Variable.Name)
            );
        }

        locals.Add(declaration.Variable);

        if (declaration.Initializer is null)
            return state with { State = state.State.MarkVariableDeclared(declaration.Variable) };

        state = state with { State = state.State.MarkVariableInitialized(declaration.Variable) };
        return VisitExpression(declaration.Initializer, state);
    }

    private FlowState VisitReturn(BoundReturnStatement statement, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        state = VisitExpression(statement.Expression, state);
        return state.AsUnreachable();
    }

    private FlowState VisitIf(BoundIfStatement statement, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        state = VisitExpression(statement.Condition, state);

        var conditionFlow = AnalyzeCondition(statement.Condition, state.State);
        var thenInputState = state with { State = conditionFlow.WhenTrue };
        var elseInputState = state with { State = conditionFlow.WhenFalse };

        if (statement.Condition.ConstantValue is { PrimitiveKind: PrimitiveKind.Bool } constant)
        {
            if (constant.AsBoolean())
            {
                state = VisitStatement(statement.ThenStatement, thenInputState);
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
                    state = VisitStatement(statement.ElseStatement, elseInputState);
                }
            }

            return state;
        }

        var thenState = VisitStatement(statement.ThenStatement, thenInputState);

        if (statement.ElseStatement is null)
        {
            return thenState.Merge(elseInputState);
        }

        var elseState = VisitStatement(statement.ElseStatement, elseInputState);
        return thenState.Merge(elseState);
    }

    private FlowState VisitWhile(BoundWhileStatement statement, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        _loopStack.Add(new LoopContext(statement.Label));

        state = VisitExpression(statement.Condition, state);

        var conditionAnalysis = AnalyzeCondition(statement.Condition, state.State);
        var enterLoopState = state with { State = conditionAnalysis.WhenTrue };
        var skipLoopState = state with { State = conditionAnalysis.WhenFalse };

        if (statement.Condition.ConstantValue is { PrimitiveKind: PrimitiveKind.Bool } constant)
        {
            if (constant.AsBoolean())
            {
                state = VisitStatement(statement.Body, enterLoopState);

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
            var bodyState = VisitStatement(statement.Body, enterLoopState);
            state = skipLoopState.Merge(bodyState);
        }

        _loopStack.RemoveAt(_loopStack.Count - 1);
        return state;
    }

    private FlowState VisitLoop(BoundLoopStatement statement, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();
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
        _cancellationToken.ThrowIfCancellationRequested();
        if (statement.Variable is not null)
        {
            state = state with
            {
                State = state.State.MarkVariableInitialized(statement.Variable.Variable),
            };
            state = VisitExpression(statement.Variable.Initializer, state);
        }

        state = statement.Initializers.Aggregate(state, (current, initializer) =>
            VisitExpression(initializer, current)
        );

        _loopStack.Add(new LoopContext(statement.Label));

        if (statement.Condition is not null)
        {
            var conditionAnalysis = AnalyzeCondition(statement.Condition, state.State);
            var enterLoopState = state with { State = conditionAnalysis.WhenTrue };
            var skipLoopState = state with { State = conditionAnalysis.WhenFalse };

            state = VisitExpression(statement.Condition, state);

            if (statement.Condition.ConstantValue is { PrimitiveKind: PrimitiveKind.Bool } constant)
            {
                if (constant.AsBoolean())
                {
                    state = VisitStatement(statement.Body, enterLoopState);

                    if (state.IsReachable)
                    {
                        state = statement.Incrementors.Aggregate(state, (current, incrementor) =>
                            VisitExpression(incrementor, current)
                        );
                    }

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
                var bodyState = VisitStatement(statement.Body, enterLoopState);

                if (bodyState.IsReachable)
                {
                    var incrementorState = statement.Incrementors.Aggregate(
                        bodyState,
                        (current, incrementor) => VisitExpression(incrementor, current)
                    );

                    bodyState = bodyState.Merge(incrementorState);
                }

                state = skipLoopState.Merge(bodyState);
            }
        }
        else
        {
            state = VisitStatement(statement.Body, state);

            if (state.IsReachable)
            {
                state = statement.Incrementors.Aggregate(state, (current, incrementor) =>
                    VisitExpression(incrementor, current)
                );
            }

            if (!_loopStack[^1].HasReachableBreak)
            {
                state = state.AsUnreachable();
            }
        }

        _loopStack.RemoveAt(_loopStack.Count - 1);
        return state;
    }

    private FlowState VisitBreak(BoundBreakStatement statement, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        ref var target = ref LookupLoop(statement.Label);
        target = target with { HasReachableBreak = true };
        return state.AsUnreachable();
    }

    private FlowState VisitContinue(BoundContinueStatement statement, FlowState state)
    {
        _cancellationToken.ThrowIfCancellationRequested();

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
            _cancellationToken.ThrowIfCancellationRequested();
            if (expression is null)
                return state;

            switch (expression)
            {
                case BoundAddressOf addressOf:
                    if (TryGetAddressedLocal(addressOf.Operand) is { } local)
                    {
                        _addressedLocals.Add(local);
                    }
                    else if (!addressOf.Operand.IsAddressable)
                    {
                        if (!_flags.HasFlag(Flags.ConstructingArguments))
                        {
                            _bindingContext.ReportDiagnostic(
                                Diagnostic.CannotTakeAddress(addressOf.Operand.Syntax.Location)
                            );
                        }
                        else if (addressOf.IsMutable)
                        {
                            _bindingContext.ReportDiagnostic(
                                Diagnostic.NoMutableTemporaryRefs(addressOf.Syntax.Location)
                            );
                        }
                    }

                    if (addressOf.IsMutable)
                    {
                        HandleMutableReferencing(
                            addressOf.Operand,
                            addressOf.Operand.Syntax.Location
                        );
                    }

                    expression = addressOf.Operand;
                    continue;

                case BoundVariableAccess { Symbol: var variable, Syntax.Location: var location }:
                    switch (state.State.GetVariableInitialization(variable))
                    {
                        case InitializationState.DefinitelyInitialized:
                            break;
                        case InitializationState.MaybeInitialized:
                            _bindingContext.ReportDiagnostic(
                                Diagnostic.NotInitializedOnAllPaths(location, variable.Name)
                            );
                            break;
                        case InitializationState.Uninitialized:
                            _bindingContext.ReportDiagnostic(
                                Diagnostic.ValueUninitialized(location, variable.Name)
                            );
                            break;
                        default:
                            throw new ArgumentException("Invalid variable state");
                    }

                    return state;

                case BoundUnaryOperation unary:
                    if (unary.Operation.IsAssigning)
                    {
                        state = HandleAssignmentOperand(
                            unary.Operand,
                            unary.Syntax.Location,
                            state
                        );
                    }
                    expression = unary.Operand;
                    continue;

                case BoundBinaryOperation binary:
                    state = VisitExpression(binary.Left, state);
                    expression = binary.Right;
                    continue;

                case BoundAssignmentOperation assignment:
                    state = HandleAssignmentOperand(
                        assignment.Left,
                        assignment.Syntax.Location,
                        state
                    );
                    state = HandleNullableAssignment(assignment.Left, assignment.Right, state);
                    state = VisitExpression(assignment.Left, state);
                    expression = assignment.Right;
                    continue;

                case BoundConditional conditional:
                {
                    state = VisitExpression(conditional.Condition, state);

                    var conditionAnalysis = AnalyzeCondition(conditional.Condition, state.State);
                    var trueState = state with { State = conditionAnalysis.WhenTrue };
                    var falseState = state with { State = conditionAnalysis.WhenFalse };

                    trueState = VisitExpression(conditional.WhenTrue, trueState);
                    falseState = VisitExpression(conditional.WhenFalse, falseState);
                    state = trueState.Merge(falseState);
                    break;
                }

                case BoundInvocation invocation:
                {
                    using var scope = new FlagScope(this, Flags.ConstructingArguments);
                    state = invocation.Arguments.Aggregate(state, (current, argument) =>
                        VisitExpression(argument, current)
                    );

                    return state;
                }

                case BoundConversion conversion:
                    if (conversion.Conversion.IsUnwrapping)
                    {
                        HandleNullableUnwrapping(conversion.Operand, state);
                    }
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

    private FlowState HandleAssignmentOperand(
        BoundExpression operand,
        Location location,
        FlowState state
    )
    {
        switch (operand)
        {
            case BoundParameterAccess { Symbol: { IsMutable: false } param }:
                _bindingContext.ReportDiagnostic(Diagnostic.CannotReassign(location, param.Name));
                return state;
            case BoundVariableAccess { Symbol: var variable }:
                if (variable.IsGlobal && _function is SynthesizedGlobalConstructor)
                    return state;

                if (variable.IsMutable)
                {
                    if (
                        state.State.GetVariableInitialization(variable)
                        == InitializationState.Undeclared
                    )
                    {
                        _bindingContext.ReportDiagnostic(
                            Diagnostic.CannotUseUndeclaredVariable(location, variable.Name)
                        );
                    }
                    return state with { State = state.State.MarkVariableInitialized(variable) };
                }

                switch (state.State.GetVariableInitialization(variable))
                {
                    case InitializationState.DefinitelyInitialized:
                        _bindingContext.ReportDiagnostic(
                            Diagnostic.CannotReassign(location, variable.Name)
                        );
                        return state;
                    case InitializationState.MaybeInitialized:
                        _bindingContext.ReportDiagnostic(
                            Diagnostic.CannotReassign(location, variable.Name)
                        );
                        return state with { State = state.State.MarkVariableInitialized(variable) };
                    case InitializationState.Uninitialized:
                        return state with { State = state.State.MarkVariableInitialized(variable) };
                    case InitializationState.Undeclared:
                        _bindingContext.ReportDiagnostic(
                            Diagnostic.CannotUseUndeclaredVariable(location, variable.Name)
                        );
                        return state with { State = state.State.MarkVariableInitialized(variable) };

                    default:
                        throw new ArgumentException("Invalid variable state");
                }
            default:
                return state;
        }
    }

    private void HandleMutableReferencing(BoundExpression operand, Location location)
    {
        switch (operand)
        {
            case BoundParameterAccess { Symbol: { IsMutable: false } param }:
                _bindingContext.ReportDiagnostic(
                    Diagnostic.CannotTakeMutableAddress(location, param.Name)
                );
                break;
            case BoundVariableAccess { Symbol: { IsMutable: false } variable }:
                _bindingContext.ReportDiagnostic(
                    Diagnostic.CannotTakeMutableAddress(location, variable.Name)
                );
                break;
        }
    }

    private static ValueSymbol? GetAssociatedSymbol(BoundExpression expression)
    {
        return expression switch
        {
            BoundVariableAccess variable => variable.Symbol,
            BoundParameterAccess parameter => parameter.Symbol,
            _ => null,
        };
    }

    private static bool NeedsNullabilityAnalysis(ValueSymbol symbol)
    {
        return NeedsNullabilityAnalysis(symbol.Type);
    }

    private static bool NeedsNullabilityAnalysis(TypeSymbol type)
    {
        return type is NullableTypeSymbol;
    }

    private static FlowState HandleNullableAssignment(
        BoundExpression assignee,
        BoundExpression operand,
        FlowState flowState
    )
    {
        var associated = GetAssociatedSymbol(assignee);
        if (associated is null || NeedsNullabilityAnalysis(associated))
            return flowState;

        return flowState with
        {
            State = flowState.State.MarkNullableState(
                associated,
                GetNullableState(operand, flowState)
            ),
        };
    }

    private static NullableState GetNullableState(BoundExpression expression, FlowState flowState)
    {
        if (!NeedsNullabilityAnalysis(expression.Type))
            return NullableState.NotNull;

        return expression switch
        {
            BoundVariableAccess variable => flowState.State.GetValueNullableState(variable.Symbol),
            BoundParameterAccess parameter => flowState.State.GetValueNullableState(
                parameter.Symbol
            ),
            BoundConversion conversion => GetNullableState(conversion.Operand, flowState),
            _ => NullableState.MaybeNull,
        };
    }

    private void HandleNullableUnwrapping(BoundExpression expression, FlowState state)
    {
        var nullableState = GetNullableState(expression, state);
        if (nullableState == NullableState.NotNull)
            return;

        _bindingContext.ReportDiagnostic(Diagnostic.MayBeNull(expression.Syntax.Location));
    }

    private ConditionFlow AnalyzeCondition(BoundExpression condition, AnalysisState state)
    {
        switch (condition)
        {
            case BoundUnaryOperation { Operation: UnaryOperation.LogicalNot, Operand: var operand }:
            {
                var inner = AnalyzeCondition(operand, state);
                return new ConditionFlow(inner.WhenFalse, inner.WhenTrue);
            }

            case BoundBinaryOperation
            {
                Operation: BinaryOperation.LogicalAnd,
                Left: var left,
                Right: var right
            }:
            {
                var leftFlow = AnalyzeCondition(left, state);
                var rightFlow = AnalyzeCondition(right, leftFlow.WhenTrue);
                return new ConditionFlow(
                    rightFlow.WhenTrue,
                    leftFlow.WhenFalse.Merge(rightFlow.WhenFalse)
                );
            }

            case BoundBinaryOperation
            {
                Operation: BinaryOperation.LogicalOr,
                Left: var left,
                Right: var right
            }:
            {
                var leftFlow = AnalyzeCondition(left, state);
                var rightFlow = AnalyzeCondition(right, leftFlow.WhenFalse);
                return new ConditionFlow(
                    leftFlow.WhenTrue.Merge(rightFlow.WhenTrue),
                    rightFlow.WhenFalse
                );
            }

            default:
            {
                if (TryAnalyzeDirectNullTest(condition, state) is { } flow)
                    return flow;

                return new ConditionFlow(state, state);
            }
        }
    }

    private ConditionFlow? TryAnalyzeDirectNullTest(BoundExpression condition, AnalysisState state)
    {
        if (
            condition
            is not BoundBinaryOperation
            {
                Operation: var operation,
                Left: var left,
                Right: var right
            }
        )
        {
            return null;
        }

        var symbol = TryGetNullCheckedSymbol(left, right) ?? TryGetNullCheckedSymbol(right, left);
        if (symbol is null)
            return null;

        return operation switch
        {
            BinaryOperation.NotEquals => new ConditionFlow(
                state.MarkNullableState(symbol, NullableState.NotNull),
                state.MarkNullableState(symbol, NullableState.MaybeNull)
            ),
            BinaryOperation.Equality => new ConditionFlow(
                state.MarkNullableState(symbol, NullableState.MaybeNull),
                state.MarkNullableState(symbol, NullableState.NotNull)
            ),
            _ => null,
        };
    }

    private static ValueSymbol? TryGetNullCheckedSymbol(
        BoundExpression value,
        BoundExpression other
    )
    {
        if (other is not BoundLiteral { Value.IsNull: true })
            return null;

        return value switch
        {
            BoundVariableAccess variable => variable.Symbol,
            BoundParameterAccess parameter => parameter.Symbol,
            _ => null,
        };
    }
}
