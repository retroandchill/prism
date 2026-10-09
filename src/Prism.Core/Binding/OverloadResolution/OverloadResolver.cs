using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Binding.Utils;
using Prism.Core.BoundTree;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Binding.OverloadResolution;

internal readonly record struct CallArgument(string? Name, BoundExpression Expression);

internal readonly record struct ResolvedOverload(
    FunctionSymbol Function,
    ImmutableArray<BoundExpression> Arguments
);

internal sealed class OverloadResolver(IOverloadResolutionHost host)
{
    private readonly record struct OverloadResolutionResult(ResolvedOverload Match, bool IsExact);

    private enum ReceiverForm
    {
        RValue,
        ImmutableReference,
        MutableReference,
    }

    private enum ReceiverQualifier
    {
        Value,
        Reference,
        MutableReference,
    }

    public ResolvedOverload ResolveInvocation(
        ImmutableArray<FunctionSymbol> candidates,
        BoundExpression? receiver,
        ImmutableArray<CallArgument> arguments,
        SyntaxNode overloadSyntax,
        BindingContext context
    )
    {
        var hadStructurallyCallableCandidate = false;
        var matches = new SortedDictionary<int, (List<ResolvedOverload>, List<ResolvedOverload>)>();

        var hasExplicitReceiver = receiver is not null;
        receiver ??= host.TryGetImplicitReceiver();
        var receiverForm = GetReceiverForm(receiver);

        foreach (var overload in candidates)
        {
            var qualifier = GetReceiverQualifier(overload);
            int candidateRank;
            if (qualifier is not null)
            {
                if (receiverForm is null)
                    continue;

                if (GetReceiverRank(receiverForm.Value, qualifier.Value) is { } r)
                    candidateRank = r;
                else
                    continue;
            }
            else if (hasExplicitReceiver)
                continue;
            else
            {
                candidateRank = 0;
            }

            if (overload.Parameters.Length < arguments.Length)
                continue;

            var mappedArguments = ArgumentMapper.TryMapArgumentsToParameters(
                host.Compilation,
                overload.Parameters.AsSpan(),
                arguments,
                overloadSyntax
            );
            if (mappedArguments is null)
                continue;

            hadStructurallyCallableCandidate = true;

            var mappedImmutable = ImmutableCollectionsMarshal.AsImmutableArray(mappedArguments);
            if (TryMatchOverload(overload, mappedImmutable, context) is not var (resolved, isExact))
            {
                continue;
            }

            var (exactMatches, convertibleMatches) = matches.GetOrAdd(candidateRank, () =>
                ([], [])
            );

            if (isExact)
            {
                exactMatches.Add(resolved);
            }
            else
            {
                convertibleMatches.Add(resolved);
            }
        }

        foreach (var (exactMatches, convertibleMatches) in matches.Values)
        {
            switch (exactMatches.Count)
            {
                case 1:
                    return exactMatches[0];
                case > 1:
                {
                    var match = exactMatches[0];
                    context.ReportDiagnostic(
                        Diagnostic.AmbiguousOverloadDefined(
                            overloadSyntax.Location,
                            GetTypeNames(match.Arguments)
                        )
                    );
                    return match;
                }
            }

            switch (convertibleMatches.Count)
            {
                case 1:
                    return convertibleMatches[0];
                case > 1:
                    var realized = RealizeSpeculativeBindingForDiagnostics(arguments, context);
                    context.ReportDiagnostic(
                        Diagnostic.AmbiguousOverloadDefined(
                            overloadSyntax.Location,
                            GetTypeNames(realized)
                        )
                    );
                    return convertibleMatches[0];
            }
        }

        var diagnosticArguments = RealizeSpeculativeBindingForDiagnostics(arguments, context);
        context.ReportDiagnostic(
            hadStructurallyCallableCandidate
                ? Diagnostic.NoOverloadForArgTypes(
                    overloadSyntax.Location,
                    GetTypeNames(diagnosticArguments)
                )
                : Diagnostic.NoOverloadMatchingArgCount(overloadSyntax.Location, arguments.Length)
        );

        return new ResolvedOverload(candidates[0], diagnosticArguments);
    }

    private static ReceiverForm? GetReceiverForm(BoundExpression? expression)
    {
        return expression switch
        {
            null => null,
            { IsAddressable: false } => ReceiverForm.RValue,
            { IsAddressable: true, IsAssignable: false } => ReceiverForm.ImmutableReference,
            { IsAddressable: true, IsAssignable: true } => ReceiverForm.MutableReference,
        };
    }

    private static ReceiverQualifier? GetReceiverQualifier(FunctionSymbol symbol)
    {
        return symbol.ReceiverType switch
        {
            null => null,
            ReferenceTypeSymbol { IsMutable: var mutable } => mutable
                ? ReceiverQualifier.MutableReference
                : ReceiverQualifier.Reference,
            _ => ReceiverQualifier.Value,
        };
    }

    private static int? GetReceiverRank(ReceiverForm actual, ReceiverQualifier qualifier)
    {
        return actual switch
        {
            ReceiverForm.RValue => qualifier switch
            {
                ReceiverQualifier.Value => 0,
                ReceiverQualifier.Reference => 1,
                ReceiverQualifier.MutableReference => null,
                _ => throw new ArgumentOutOfRangeException(nameof(qualifier), qualifier, null),
            },
            ReceiverForm.ImmutableReference => qualifier switch
            {
                ReceiverQualifier.Reference => 0,
                ReceiverQualifier.Value => 1,
                ReceiverQualifier.MutableReference => null,
                _ => throw new ArgumentOutOfRangeException(nameof(qualifier), qualifier, null),
            },
            ReceiverForm.MutableReference => qualifier switch
            {
                ReceiverQualifier.MutableReference => 0,
                ReceiverQualifier.Reference => 1,
                ReceiverQualifier.Value => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(qualifier), qualifier, null),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(actual), actual, null),
        };
    }

    private OverloadResolutionResult? TryMatchOverload(
        FunctionSymbol overload,
        ImmutableArray<BoundExpression> arguments,
        BindingContext context
    )
    {
        BoundExpression[]? remappedArgs = null;
        var isExactMatch = true;
        foreach (var (i, argument) in arguments.AsValueEnumerable().Index())
        {
            var parameter = overload.Parameters[i];

            if (argument.Type == parameter.Type)
                continue;

            BoundExpression newExpression;
            bool isSpeculative;
            if (argument is BoundSpeculativeExpression speculative)
            {
                isExactMatch &= parameter.Type == speculative.DefaultType;
                newExpression = host.ApplySpeculativeBinding(speculative, parameter.Type, context);
                isSpeculative = true;
            }
            else
            {
                newExpression = argument;
                isSpeculative = false;
            }

            var conversion = host.ClassifyConversion(newExpression.Type, parameter.Type);
            if (!conversion.IsImplicit)
            {
                return null;
            }

            if (!isSpeculative)
            {
                isExactMatch &= conversion.IsIdentity;
            }

            newExpression = host.AddConversion(newExpression, parameter.Type, conversion, context);

            if (remappedArgs is null)
            {
                remappedArgs = new BoundExpression[arguments.Length];
                arguments.CopyTo(remappedArgs);
            }

            remappedArgs[i] = newExpression;
        }

        return remappedArgs is not null
            ? new OverloadResolutionResult(
                new ResolvedOverload(
                    overload,
                    ImmutableCollectionsMarshal.AsImmutableArray(remappedArgs)
                ),
                isExactMatch
            )
            : new OverloadResolutionResult(new ResolvedOverload(overload, arguments), isExactMatch);
    }

    private ImmutableArray<BoundExpression> RealizeSpeculativeBindingForDiagnostics(
        ImmutableArray<CallArgument> arguments,
        BindingContext context
    )
    {
        var realized = new BoundExpression[arguments.Length];
        foreach (var (i, argument) in arguments.AsValueEnumerable().Index())
        {
            realized[i] = host.ApplySpeculativeBinding(argument.Expression, null, context);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(realized);
    }

    private static string GetTypeNames(ImmutableArray<BoundExpression> arguments)
    {
        return string.Join(", ", arguments.Select(a => a.Type.Name));
    }
}
