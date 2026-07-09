// @file SymbolBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic.Binding;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Semantic;

internal readonly record struct BindingContext(
    IDiagnosticSink Diagnostics,
    ResolutionContext Resolution
);

internal sealed class SemanticBinder(SemanticModel semanticModel)
{
    private ValueSymbol ResolveVariable(Name name, LocalScope scope)
    {
        while (true)
        {
            var local = scope.GetLocal(name);
            if (local is not null)
                return local;

            if (scope.OuterLocal is null)
                return ResolveVariable(name, scope.OuterDeclaration);
            scope = scope.OuterLocal;
        }
    }

    private ValueSymbol ResolveVariable(Name name, DeclarationScope scope)
    {
        while (true)
        {
            var candidates = scope.GetDeclaredHere(name);
            var count = 0;
            ValueSymbol bestMatch = semanticModel.UnresolvedValueSymbol;
            foreach (var candidate in candidates)
            {
                if (candidate is not ValueSymbol valueSymbol)
                    continue;

                count++;
                if (count == 1)
                    bestMatch = valueSymbol;
                else
                {
                    return semanticModel.UnresolvedValueSymbol;
                }
            }

            if (count != 0 || scope.Parent is null)
                return bestMatch;
            scope = scope.Parent;
        }
    }

    public async ValueTask<BoundExpression> BindAsync(
        ExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        TypeSymbol? expectedType = null,
        CancellationToken cancellationToken = default
    )
    {
        var bound = expression switch
        {
            BooleanLiteralExpressionSyntax literal => new BoundBoolLiteralExpression(
                literal,
                semanticModel,
                literal.Value
            ),
            IntegerLiteralExpressionSyntax literal => new BoundIntegerLiteralExpression(
                literal,
                semanticModel,
                literal.Value,
                literal.Suffix
            ),
            FloatLiteralExpressionSyntax literal => new BoundFloatLiteralExpression(
                literal,
                semanticModel,
                literal.Value,
                literal.Suffix
            ),
            StringLiteralExpressionSyntax literal => new BoundStringLiteralExpression(
                literal,
                semanticModel,
                literal.Value
            ),
            IdentifierExpressionSyntax identifier => await BindAsync(
                identifier,
                scope,
                context,
                cancellationToken
            ),
            UnaryExpressionSyntax unary => await BindAsync(
                unary,
                scope,
                context,
                cancellationToken
            ),
            BinaryExpressionSyntax binary => await BindAsync(
                binary,
                scope,
                context,
                cancellationToken: cancellationToken
            ),
            TernaryExpressionSyntax ternary => await BindAsync(
                ternary,
                scope,
                context,
                expectedType,
                cancellationToken
            ),
            _ => throw new NotImplementedException(),
        };

        return expectedType is null
            ? bound
            : BindIfImplicitlyConvertable(bound, expectedType, context);
    }

    private BoundExpression BindIfImplicitlyConvertable(
        BoundExpression expression,
        TypeSymbol expectedType,
        BindingContext context
    )
    {
        var conversionKind = ClassifyConversion(expectedType, expression.Type);
        switch (conversionKind)
        {
            case ConversionKind.Identity:
                // No conversion needed
                break;
            case ConversionKind.Implicit:
                return new BoundConversionExpression(
                    expression.Syntax,
                    expectedType,
                    expression,
                    true
                );
            case ConversionKind.Explicit or ConversionKind.None:
                /*
                context.Diagnostics.NoImplicitConversion(
                    expression.Syntax.Range,
                    expression.Type.Name,
                    expectedType.Name
                );
                */
                break;
        }

        return expression;
    }

    private async ValueTask<BoundVariableExpression> BindAsync(
        IdentifierExpressionSyntax identifier,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var variable = ResolveVariable(identifier.Name, scope);

        var type = await semanticModel.ResolveValueTypeAsync(variable, context, cancellationToken);

        return new BoundVariableExpression(variable, type);
    }

    private async ValueTask<BoundExpression> BindAsync(
        UnaryExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var operand = await BindAsync(
            expression.Operand,
            scope,
            context,
            cancellationToken: cancellationToken
        );

        // Special case for the negation of integer and floating-point literals, since we want to collapse them into
        // a single literal.
        if (expression.Operator == UnaryOperator.Negate)
        {
            switch (operand)
            {
                case BoundIntegerLiteralExpression bound:
                    // TODO: Check to make sure we aren't negating an unsigned integer literal
                    return new BoundIntegerLiteralExpression(
                        expression,
                        semanticModel,
                        -bound.Value,
                        bound.Suffix
                    );
                case BoundFloatLiteralExpression bound:
                    return new BoundFloatLiteralExpression(
                        expression,
                        semanticModel,
                        -bound.Value,
                        bound.Suffix
                    );
            }
        }

        var isValidType = false;
        TypeSymbol targetType = semanticModel.ErrorTypeSymbol;
        if (operand.Type is NamedTypeSymbol { BuiltInType: { } builtInType })
        {
            if (Operators.TryResolveUnary(expression.Operator, builtInType, out var resultType))
            {
                isValidType = true;
                targetType = semanticModel.BuiltInTypes.GetSymbol(resultType);

                if (resultType != builtInType)
                {
                    operand = new BoundConversionExpression(
                        operand.Syntax,
                        targetType,
                        operand,
                        true
                    );
                }
            }
        }

        if (!isValidType)
        {
            /*
            context.Diagnostics.UnaryOperatorUndefined(expression.Range, operand.Type.Name);
            */
        }

        return new BoundUnaryExpression(expression, operand, expression.Operator, targetType);
    }

    private async ValueTask<BoundExpression> BindAsync(
        BinaryExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var left = await BindAsync(
            expression.Left,
            scope,
            context,
            cancellationToken: cancellationToken
        );
        var right = await BindAsync(
            expression.Right,
            scope,
            context,
            cancellationToken: cancellationToken
        );

        if (expression.Operator.IsAssignmentOperator)
        {
            return ConstructAssignment(expression, left, right, context);
        }

        return ConstructBinaryExpression(expression, left, right, context);
    }

    private async Task<BoundConditionalExpression> BindAsync(
        TernaryExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        TypeSymbol? targetType,
        CancellationToken cancellationToken = default
    )
    {
        var condition = await BindAsync(
            expression.Condition,
            scope,
            context,
            cancellationToken: cancellationToken
        );
        var ifTrue = await BindAsync(
            expression.TrueExpression,
            scope,
            context,
            cancellationToken: cancellationToken
        );
        var ifFalse = await BindAsync(
            expression.FalseExpression,
            scope,
            context,
            cancellationToken: cancellationToken
        );

        condition = BindIfImplicitlyConvertable(
            condition,
            semanticModel.BuiltInTypes.Bool,
            context
        );

        targetType ??= FindCommonBase(ifTrue.Type, ifFalse.Type);
        if (targetType is not null)
        {
            ifTrue = BindIfImplicitlyConvertable(ifTrue, targetType, context);
            ifFalse = BindIfImplicitlyConvertable(ifFalse, targetType, context);
        }
        else if (!ReferenceEquals(ifTrue.Type, ifFalse.Type))
        {
            /*
            context.Diagnostics.NoMutualImplicitConversion(
                expression.Range,
                ifTrue.Type.Name,
                ifFalse.Type.Name
            );
            */
        }

        return new BoundConditionalExpression(expression, condition, ifTrue, ifFalse, ifTrue.Type);
    }

    private async ValueTask<BoundInvocationExpression> BindAsync(
        InvocationExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var callee = await BindAsync(
            expression.Callee,
            scope,
            context,
            cancellationToken: cancellationToken
        );
        var arguments = await expression
            .Arguments.ToAsyncEnumerable()
            .Select((syntax, token) => BindAsync(syntax, scope, context, cancellationToken: token))
            .ToArrayAsync(cancellationToken);

        switch (callee)
        {
            case BoundFunctionReferenceExpression functionReference:
                return await BindCallableAsync(
                    expression,
                    functionReference,
                    arguments,
                    scope,
                    context,
                    cancellationToken
                );
            case BoundOverloadSetExpression overloadSet:
                return await BindCallableAsync(
                    expression,
                    overloadSet,
                    arguments,
                    scope,
                    context,
                    cancellationToken
                );
            default:
                /*
                context.Diagnostics.NotInvocable(expression.Range);
                */
                return new BoundInvocationExpression(
                    expression,
                    semanticModel.UnresolvedCallableSymbol,
                    semanticModel.ErrorTypeSymbol,
                    ImmutableCollectionsMarshal.AsImmutableArray(arguments)
                );
        }
    }

    private BoundBinaryExpression ConstructBinaryExpression(
        BinaryExpressionSyntax expression,
        BoundExpression left,
        BoundExpression right,
        BindingContext context
    )
    {
        var boundOperator = expression.Operator.ToBoundOperator();

        if (
            left.Type is NamedTypeSymbol { BuiltInType: { } leftType }
            && right.Type is NamedTypeSymbol { BuiltInType: { } rightType }
        )
        {
            if (
                Operators.TryResolveBinary(
                    boundOperator,
                    leftType,
                    rightType,
                    semanticModel.TargetPlatform,
                    out var result
                )
            )
            {
                if (leftType != result.LeftType)
                {
                    left = new BoundConversionExpression(
                        left.Syntax,
                        semanticModel.BuiltInTypes.GetSymbol(result.LeftType),
                        left,
                        true
                    );
                }

                if (rightType != result.RightType)
                {
                    right = new BoundConversionExpression(
                        right.Syntax,
                        semanticModel.BuiltInTypes.GetSymbol(result.RightType),
                        right,
                        true
                    );
                }
                return new BoundBinaryExpression(
                    expression,
                    left,
                    right,
                    boundOperator,
                    semanticModel.BuiltInTypes.GetSymbol(result.ResultType)
                );
            }
        }

        /*
        context.Diagnostics.BinaryOperatorUndefined(
            expression.Range,
            left.Type.Name,
            right.Type.Name
        );
        */

        return new BoundBinaryExpression(
            expression,
            left,
            right,
            boundOperator,
            semanticModel.ErrorTypeSymbol
        );
    }

    private BoundAssignmentExpression ConstructAssignment(
        BinaryExpressionSyntax expression,
        BoundExpression left,
        BoundExpression right,
        BindingContext context
    )
    {
        var value =
            expression.Operator == BinaryOperator.Assign
                ? right
                : ConstructBinaryExpression(expression, left, right, context);

        return new BoundAssignmentExpression(
            expression,
            semanticModel,
            left,
            BindIfImplicitlyConvertable(value, left.Type, context)
        );
    }

    private async ValueTask<BoundInvocationExpression> BindCallableAsync(
        InvocationExpressionSyntax syntax,
        BoundFunctionReferenceExpression functionRef,
        BoundExpression[] arguments,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var (function, returnType, parameterTypes) = await GetOverloadInformationAsync(
            functionRef.Function,
            context,
            cancellationToken
        );

        if (parameterTypes.Length != arguments.Length)
        {
            /*
            context.Diagnostics.WrongNumberOfArguments(
                syntax.ArgumentsRange,
                function.Name,
                parameterTypes.Length,
                arguments.Length
            );
            */
        }
        else
        {
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                ref var argument = ref arguments[i];
                var parameterType = parameterTypes[i].Type;

                argument = BindIfImplicitlyConvertable(argument, parameterType, context);
            }
        }

        return new BoundInvocationExpression(
            syntax,
            function,
            returnType,
            ImmutableCollectionsMarshal.AsImmutableArray(arguments)
        );
    }

    private async ValueTask<BoundInvocationExpression> BindCallableAsync(
        InvocationExpressionSyntax syntax,
        BoundOverloadSetExpression functionRef,
        BoundExpression[] arguments,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        // We want to bind all overload signatures before we start so we can possibly report diagnostics against all of them
        var overloadCandidates = await functionRef
            .Functions.ToAsyncEnumerable()
            .Select((func, ct) => GetOverloadInformationAsync(func, context, ct))
            .ToArrayAsync(cancellationToken);
        foreach (var (function, returnType, parameters) in overloadCandidates)
        {
            if (parameters.Length != arguments.Length)
            {
                continue;
            }

            if (
                !arguments
                    .Zip(parameters)
                    .All(args => CanConvert(args.Second.Type, args.First.Type))
            )
            {
                continue;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                ref var argument = ref arguments[i];
                var parameterType = parameters[i].Type;

                argument = BindIfImplicitlyConvertable(argument, parameterType, context);
            }

            return new BoundInvocationExpression(
                syntax,
                function,
                returnType,
                ImmutableCollectionsMarshal.AsImmutableArray(arguments)
            );
        }

        /*
        context.Diagnostics.NoSuitableOverload(
            syntax.ArgumentsRange,
            functionRef.Functions[0].Name
        );
        */

        return new BoundInvocationExpression(
            syntax,
            semanticModel.UnresolvedCallableSymbol,
            semanticModel.ErrorTypeSymbol,
            ImmutableCollectionsMarshal.AsImmutableArray(arguments)
        );
    }

    private readonly record struct OverloadParameter(TypeSymbol Type);

    private readonly record struct OverloadCandidate(
        CallableSymbol Function,
        TypeSymbol ReturnType,
        ImmutableArray<OverloadParameter> Parameters
    );

    private async ValueTask<OverloadCandidate> GetOverloadInformationAsync(
        CallableSymbol function,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var returnType = await semanticModel.ResolveFunctionReturnTypeAsync(
            function,
            context,
            cancellationToken
        );
        var parameterTypes = await function
            .Parameters.ToAsyncEnumerable()
            .Select((param, token) => semanticModel.ResolveValueTypeAsync(param, context, token))
            .Select(x => new OverloadParameter(x))
            .ToArrayAsync(cancellationToken);

        return new OverloadCandidate(
            function,
            returnType,
            ImmutableCollectionsMarshal.AsImmutableArray(parameterTypes)
        );
    }

    private ConversionKind ClassifyConversion(TypeSymbol to, TypeSymbol from)
    {
        if (ReferenceEquals(to, from))
        {
            return ConversionKind.Identity;
        }

        if (
            to is NamedTypeSymbol { BuiltInType: { } toBuiltIn }
            && from is NamedTypeSymbol { BuiltInType: { } fromBuiltIn }
        )
        {
            return toBuiltIn.ClassifyConversion(fromBuiltIn, semanticModel.TargetPlatform);
        }

        return ConversionKind.None;
    }

    private bool CanConvert(TypeSymbol to, TypeSymbol from)
    {
        return ClassifyConversion(to, from) is ConversionKind.Implicit or ConversionKind.Identity;
    }

    private TypeSymbol? FindCommonBase(TypeSymbol type1, TypeSymbol type2)
    {
        var conversion1 = ClassifyConversion(type1, type2);
        var conversion2 = ClassifyConversion(type2, type1);

        if (conversion1 == ConversionKind.Implicit)
        {
            return type1;
        }

        return conversion2 == ConversionKind.Implicit ? type2 : null;
    }
}
