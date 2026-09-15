// @file AnalysisState.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Utils;

namespace Prism.Core.FlowAnalysis;

internal enum InitializationState : byte
{
    DefinitelyInitialized,
    MaybeInitialized,
    Uninitialized,
}

internal enum NullableState : byte
{
    NotNull,
    MaybeNull,
}

internal readonly record struct VariableAnalysisState(InitializationState Initialization)
{
    public static VariableAnalysisState Uninitialized { get; } =
        new(InitializationState.Uninitialized);

    public VariableAnalysisState Merge(VariableAnalysisState other)
    {
        return Initialization == other.Initialization
            ? this
            : new VariableAnalysisState(InitializationState.MaybeInitialized);
    }
}

internal readonly record struct ValueNullableState(NullableState Null)
{
    public static ValueNullableState DefaultState(ValueSymbol symbol)
    {
        return symbol.Type is NullableTypeSymbol
            ? new ValueNullableState(NullableState.MaybeNull)
            : new ValueNullableState(NullableState.NotNull);
    }

    public ValueNullableState Merge(ValueNullableState other)
    {
        return Null == other.Null ? this : new ValueNullableState(NullableState.MaybeNull);
    }
}

internal sealed record AnalysisState
{
    private ImmutableDictionary<VariableSymbol, VariableAnalysisState> Variables { get; init; }
    private ImmutableDictionary<ValueSymbol, ValueNullableState> Nullability { get; init; }

    private AnalysisState(
        ImmutableDictionary<VariableSymbol, VariableAnalysisState> variables,
        ImmutableDictionary<ValueSymbol, ValueNullableState> nullability
    )
    {
        Variables = variables;
        Nullability = nullability;
    }

    public static AnalysisState Empty { get; } =
        new(
            ImmutableDictionary.Create<VariableSymbol, VariableAnalysisState>(
                ReferenceEqualityComparer.Instance
            ),
            ImmutableDictionary.Create<ValueSymbol, ValueNullableState>(
                ReferenceEqualityComparer.Instance
            )
        );

    public InitializationState GetVariableInitialization(VariableSymbol variable)
    {
        if (variable.IsGlobal)
            return InitializationState.DefinitelyInitialized;

        return Variables.TryGetValue(variable, out var variableState)
            ? variableState.Initialization
            : InitializationState.Uninitialized;
    }

    public AnalysisState MarkVariableInitialized(VariableSymbol variable)
    {
        var variables = Variables;
        if (Variables.TryGetValue(variable, out var variableState))
        {
            if (variableState.Initialization == InitializationState.DefinitelyInitialized)
                return this;

            variables = variables.SetItem(
                variable,
                new VariableAnalysisState(InitializationState.DefinitelyInitialized)
            );
        }
        else
        {
            variables = variables.Add(
                variable,
                new VariableAnalysisState(InitializationState.DefinitelyInitialized)
            );
        }

        return ReferenceEquals(variables, Variables) ? this : this with { Variables = variables };
    }

    public NullableState GetValueNullableState(ValueSymbol value)
    {
        var state = Nullability.GetValueOrDefault(value, ValueNullableState.DefaultState);
        return state.Null;
    }

    public AnalysisState MarkNullableState(ValueSymbol symbol, NullableState nullableState)
    {
        var nullability = Nullability.SetItem(symbol, new ValueNullableState(nullableState));
        if (ReferenceEquals(Nullability, nullability))
            return this;

        return this with
        {
            Nullability = nullability,
        };
    }

    public AnalysisState Merge(AnalysisState other)
    {
        if (ReferenceEquals(this, other))
            return this;

        var mergedVariables = MergeVariables(other);
        var mergedNullability = MergeNullableStates(other);
        return mergedVariables == Variables && mergedNullability == Nullability
            ? this
            : new AnalysisState(mergedVariables, mergedNullability);
    }

    private ImmutableDictionary<VariableSymbol, VariableAnalysisState> MergeVariables(
        AnalysisState other
    )
    {
        ImmutableDictionary<VariableSymbol, VariableAnalysisState>.Builder? builder = null;

        foreach (var (variable, otherState) in other.Variables)
        {
            var thisState = Variables.GetValueOrDefault(
                variable,
                VariableAnalysisState.Uninitialized
            );

            var mergedState = thisState.Merge(otherState);

            if (mergedState == thisState)
                continue;

            builder ??= Variables.ToBuilder();

            if (mergedState == VariableAnalysisState.Uninitialized)
                builder.Remove(variable);
            else
                builder[variable] = mergedState;
        }

        foreach (var (variable, thisState) in Variables)
        {
            if (other.Variables.ContainsKey(variable))
                continue;

            var mergedState = thisState.Merge(VariableAnalysisState.Uninitialized);

            if (mergedState.Equals(thisState))
                continue;

            builder ??= Variables.ToBuilder();

            if (mergedState == VariableAnalysisState.Uninitialized)
                builder.Remove(variable);
            else
                builder[variable] = mergedState;
        }

        return builder is null ? Variables : builder.ToImmutable();
    }

    private ImmutableDictionary<ValueSymbol, ValueNullableState> MergeNullableStates(
        AnalysisState other
    )
    {
        ImmutableDictionary<ValueSymbol, ValueNullableState>.Builder? builder = null;

        foreach (var (symbol, otherState) in other.Nullability)
        {
            var thisState = Nullability.GetValueOrDefault(symbol, ValueNullableState.DefaultState);

            var mergedState = thisState.Merge(otherState);

            if (mergedState == thisState)
                continue;

            builder ??= Nullability.ToBuilder();

            builder[symbol] = mergedState;
        }

        foreach (var (symbol, thisState) in Nullability)
        {
            if (other.Nullability.ContainsKey(symbol))
                continue;

            var mergedState = thisState.Merge(ValueNullableState.DefaultState(symbol));

            if (mergedState.Equals(thisState))
                continue;

            builder ??= Nullability.ToBuilder();
            builder[symbol] = mergedState;
        }

        return builder is null ? Nullability : builder.ToImmutable();
    }
}
