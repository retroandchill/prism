// @file SymbolBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Prism.Core.Ast;
using Prism.Core.Semantic.Binding;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;
using Prism.Core.Utils;

namespace Prism.Core.Semantic;

internal readonly record struct BindingContext(IDiagnosticSink Diagnostics, ResolutionContext Resolution);

internal sealed class SemanticBinder(SemanticModel semanticModel)
{
    private ValueSymbol ResolveVariable(Name name, LocalScope scope)
    {
        while (true)
        {
            var local = scope.GetLocal(name);
            if (local is not null) return local;

            if (scope.OuterLocal is null) return ResolveVariable(name, scope.OuterDeclaration);
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
                if (candidate is not ValueSymbol valueSymbol) continue;

                count++;
                if (count == 1)
                    bestMatch = valueSymbol;
                else
                {
                    return semanticModel.UnresolvedValueSymbol;
                }
            }

            if (count != 0 || scope.Parent is null) return bestMatch;
            scope = scope.Parent;
        }
    }
    
    public async ValueTask<BoundExpression> BindAsync(
        ExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        return expression switch
        {
            BooleanLiteralExpressionSyntax literal => new BoundBoolLiteralExpression
            {
                Value = literal.Value,
                Type = semanticModel.BuiltInTypes.Bool,
                Syntax = literal,
            },
            IntegerLiteralExpressionSyntax literal => new BoundIntegerLiteralExpression
            {
                Value = literal.Value,
                Suffix = literal.Suffix,
                Type = semanticModel.BuiltInTypes.GetTypeSymbol(literal.Suffix),
                Syntax = literal,
            },
            FloatLiteralExpressionSyntax literal => new BoundFloatLiteralExpression
            {
                Value = literal.Value,
                Suffix = literal.Suffix,
                Type = semanticModel.BuiltInTypes.GetTypeSymbol(literal.Suffix),
                Syntax = literal,
            },
            StringLiteralExpressionSyntax literal => new BoundStringLiteralExpression
            {
                Value = literal.Value,
                Type = semanticModel.BuiltInTypes.Str,
                Syntax = literal,
            },
            IdentifierExpressionSyntax identifier => await BindAsync(
                identifier,
                scope,
                context, cancellationToken),
            UnaryExpressionSyntax unary => await BindAsync(unary, scope, context, cancellationToken),
            BinaryExpressionSyntax binary => await BindAsync(binary, scope, context, cancellationToken),
            _ => throw new NotImplementedException(),
        };
    }

    private async ValueTask<BoundVariableExpression> BindAsync(
        IdentifierExpressionSyntax identifier,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var variable = ResolveVariable(identifier.Name, scope);

        var type = await semanticModel.GetValueTypeAsync(variable, context, cancellationToken);

        return new BoundVariableExpression
        {
            Symbol = variable,
            Syntax = identifier,
            Type = type
        };
    }

    private async ValueTask<BoundExpression> BindAsync(
        UnaryExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var operand = await BindAsync(expression.Operand, scope, context, cancellationToken);

        // Special case for the negation of integer and floating-point literals, since we want to collapse them into
        // a single literal.
        if (expression.Operator == UnaryOperator.Negate)
        {
            switch (operand)
            {
                case BoundIntegerLiteralExpression bound:
                    // TODO: Check to make sure we aren't negating an unsigned integer literal
                    return new BoundIntegerLiteralExpression
                    {
                        Value = -bound.Value,
                        Suffix = bound.Suffix,
                        Type = bound.Type,
                        Syntax = expression,
                    };
                case BoundFloatLiteralExpression bound:
                    return new BoundFloatLiteralExpression
                    {
                        Value = -bound.Value,
                        Suffix = bound.Suffix,
                        Type = bound.Type,
                        Syntax = expression,
                    };
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
                    operand = new BoundConversionExpression
                    {
                        Input = operand,
                        Type = semanticModel.BuiltInTypes.GetSymbol(resultType),
                        IsImplicit = true,
                        Syntax = operand.Syntax,
                    };
                }
            }
        }

        if (!isValidType)
        {
            context.Diagnostics.Report(
                new Diagnostic
                {
                    Descriptor = SemanticDiagnostics.UnaryOperatorUndefined,
                    Range = expression.Range,
                    Arguments = [operand.Type.Name],
                }
            );
        }

        return new BoundUnaryExpression
        {
            Operator = expression.Operator,
            Operand = operand,
            Type = targetType,
            Syntax = expression,
        };
    }

    private async Task<BoundBinaryExpression> BindAsync(
        BinaryExpressionSyntax expression,
        LocalScope scope,
        BindingContext context,
        CancellationToken cancellationToken = default
    )
    {
        var left = await BindAsync(expression.Left, scope, context, cancellationToken);
        var right = await BindAsync(expression.Right, scope, context, cancellationToken);

        if (
            left.Type is NamedTypeSymbol { BuiltInType: { } leftType }
            && right.Type is NamedTypeSymbol { BuiltInType: { } rightType }
        )
        {
            if (
                Operators.TryResolveBinary(
                    expression.Operator,
                    leftType,
                    rightType,
                    semanticModel.TargetPlatform,
                    out var result
                )
            )
            {
                if (leftType != result.LeftType)
                {
                    left = new BoundConversionExpression
                    {
                        Input = left,
                        Type = semanticModel.BuiltInTypes.GetSymbol(result.LeftType),
                        IsImplicit = true,
                        Syntax = left.Syntax,
                    };
                }

                if (rightType != result.RightType)
                {
                    right = new BoundConversionExpression
                    {
                        Input = right,
                        Type = semanticModel.BuiltInTypes.GetSymbol(result.RightType),
                        IsImplicit = true,
                        Syntax = right.Syntax,
                    };
                }
                return new BoundBinaryExpression
                {
                    Operator = expression.Operator,
                    Left = left,
                    Right = right,
                    Type = semanticModel.BuiltInTypes.GetSymbol(result.ResultType),
                    Syntax = expression,
                };
            }
        }

        context.Diagnostics.Report(
            new Diagnostic
            {
                Descriptor = SemanticDiagnostics.BinaryOperatorUndefined,
                Range = expression.Range,
                Arguments = [left.Type.Name, right.Type.Name],
            }
        );

        return new BoundBinaryExpression
        {
            Operator = expression.Operator,
            Left = left,
            Right = right,
            Type = semanticModel.ErrorTypeSymbol,
            Syntax = expression,
        };
    }
}
