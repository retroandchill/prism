// @file AnalysisState.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal enum InitializationState : byte
{
    DefinitelyInitialized,
    MaybeInitialized,
    Uninitialized,
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

internal sealed record AnalysisState
{
    private ImmutableDictionary<VariableSymbol, VariableAnalysisState> Variables { get; init; }

    private AnalysisState(ImmutableDictionary<VariableSymbol, VariableAnalysisState> variables)
    {
        Variables = variables;
    }

    public static AnalysisState Empty { get; } =
        new(
            ImmutableDictionary.Create<VariableSymbol, VariableAnalysisState>(
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

        return ReferenceEquals(variables, Variables) ? this : new AnalysisState(variables);
    }

    public AnalysisState Merge(AnalysisState other)
    {
        if (ReferenceEquals(this, other))
            return this;

        if (Variables.Count == 0)
            return other == Empty ? this : MergeIntoEmpty(other);

        if (other.Variables.Count == 0)
            return MergeIntoEmpty(this);

        var result = Variables;
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

            builder ??= result.ToBuilder();

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

            builder ??= result.ToBuilder();

            if (mergedState == VariableAnalysisState.Uninitialized)
                builder.Remove(variable);
            else
                builder[variable] = mergedState;
        }

        if (builder is null)
            return this;

        var mergedVariables = builder.ToImmutable();
        return mergedVariables == Variables ? this : new AnalysisState(mergedVariables);
    }

    private static AnalysisState MergeIntoEmpty(AnalysisState nonEmpty)
    {
        ImmutableDictionary<VariableSymbol, VariableAnalysisState>.Builder? builder = null;

        foreach (var (variable, state) in nonEmpty.Variables)
        {
            var mergedState = VariableAnalysisState.Uninitialized.Merge(state);

            if (mergedState == state)
                continue;

            builder ??= nonEmpty.Variables.ToBuilder();

            if (mergedState == VariableAnalysisState.Uninitialized)
                builder.Remove(variable);
            else
                builder[variable] = mergedState;
        }

        return builder is not null ? new AnalysisState(builder.ToImmutable()) : nonEmpty;
    }
}
