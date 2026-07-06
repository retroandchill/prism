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

public sealed class SemanticBinder(SemanticModel semanticModel)
{
    private readonly ConcurrentDictionary<SyntaxNode, BoundNode> _boundCache = new(
        ReferenceEqualityComparer.Instance
    );

    private VariableSymbol ResolveVariable(
        Name name,
        DeclarationScope scope,
        CompilationContext context
    )
    {
        throw new NotImplementedException();
    }

    public BoundNode BindNode(SyntaxNode node, DeclarationScope scope, CompilationContext context)
    {
        var expression = node switch
        {
            ExpressionSyntax expressionSyntax => BindExpression(expressionSyntax, scope, context),
            _ => throw new NotImplementedException(),
        };
        return _boundCache.GetOrAdd(node, expression);
    }

    private BoundExpression BindExpression(
        ExpressionSyntax expression,
        DeclarationScope scope,
        CompilationContext context
    )
    {
        return expression switch
        {
            BooleanLiteralExpressionSyntax literal => new BoundBoolLiteralExpression
            {
                Value = literal.Value,
                Type = BuiltInTypes.Bool,
                Syntax = literal,
            },
            IntegerLiteralExpressionSyntax literal => new BoundIntegerLiteralExpression
            {
                Value = literal.Value,
                Suffix = literal.Suffix,
                Type = literal.Suffix.GetTypeSymbol(),
                Syntax = literal,
            },
            FloatLiteralExpressionSyntax literal => new BoundFloatLiteralExpression
            {
                Value = literal.Value,
                Suffix = literal.Suffix,
                Type = literal.Suffix.GetTypeSymbol(),
                Syntax = literal,
            },
            StringLiteralExpressionSyntax literal => new BoundStringLiteralExpression
            {
                Value = literal.Value,
                Type = BuiltInTypes.Str,
                Syntax = literal,
            },
            IdentifierExpressionSyntax identifier => BindIdentifierExpression(
                identifier,
                scope,
                context
            ),
            UnaryExpressionSyntax unary => BindUnaryExpression(unary, scope, context),
            BinaryExpressionSyntax binary => BindBinaryExpression(binary, scope, context),
            _ => throw new NotImplementedException(),
        };
    }

    private BoundVariableExpression BindIdentifierExpression(
        IdentifierExpressionSyntax identifier,
        DeclarationScope scope,
        CompilationContext context
    )
    {
        var variable = ResolveVariable(identifier.Name, scope, context);

        return new BoundVariableExpression
        {
            Symbol = variable,
            Syntax = identifier,
            Type = variable.Type,
        };
    }

    private BoundExpression BindUnaryExpression(
        UnaryExpressionSyntax expression,
        DeclarationScope scope,
        CompilationContext context
    )
    {
        var operand = (BoundExpression)BindNode(expression.Operand, scope, context);

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
        TypeSymbol targetType = ErrorTypeSymbol.Default;
        if (operand.Type is NamedTypeSymbol { BuiltInType: { } builtInType })
        {
            if (Operators.TryResolveUnary(expression.Operator, builtInType, out var resultType))
            {
                isValidType = true;
                targetType = resultType.GetSymbol();

                if (resultType != builtInType)
                {
                    operand = new BoundConversionExpression
                    {
                        Input = operand,
                        Type = resultType.GetSymbol(),
                        IsImplicit = true,
                        Syntax = operand.Syntax,
                    };
                }
            }
        }

        if (!isValidType)
        {
            context.ReportDiagnostic(
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

    private BoundBinaryExpression BindBinaryExpression(
        BinaryExpressionSyntax expression,
        DeclarationScope scope,
        CompilationContext context
    )
    {
        var left = BindExpression(expression.Left, scope, context);
        var right = BindExpression(expression.Right, scope, context);

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
                    context.Platform,
                    out var result
                )
            )
            {
                if (leftType != result.LeftType)
                {
                    left = new BoundConversionExpression
                    {
                        Input = left,
                        Type = result.LeftType.GetSymbol(),
                        IsImplicit = true,
                        Syntax = left.Syntax,
                    };
                }

                if (rightType != result.RightType)
                {
                    right = new BoundConversionExpression
                    {
                        Input = right,
                        Type = result.RightType.GetSymbol(),
                        IsImplicit = true,
                        Syntax = right.Syntax,
                    };
                }
                return new BoundBinaryExpression
                {
                    Operator = expression.Operator,
                    Left = left,
                    Right = right,
                    Type = result.ResultType.GetSymbol(),
                    Syntax = expression,
                };
            }
        }

        context.ReportDiagnostic(
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
            Type = ErrorTypeSymbol.Default,
            Syntax = expression,
        };
    }
}
