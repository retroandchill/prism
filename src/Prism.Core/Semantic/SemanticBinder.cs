// @file SymbolBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Semantic.Binding;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Semantic;

public sealed class SemanticBinder
{
    private readonly ConcurrentDictionary<DeclarationSyntax, Lazy<Symbol>> _symbolCache = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly ConcurrentDictionary<SyntaxNode, BoundNode> _boundCache = new(
        ReferenceEqualityComparer.Instance
    );

    public Symbol GetSymbol(SymbolDeclaration declaration, CompilationContext context) =>
        GetSymbol(declaration, new BindingContext(context));

    private Symbol GetSymbol(SymbolDeclaration declaration, BindingContext context)
    {
        var lazy = _symbolCache.GetOrAdd(
            declaration.Syntax,
            static (_, state) =>
            {
                return new Lazy<Symbol>(() =>
                {
                    if (!state.Context.EnterInference(state.Declaration))
                    {
                        state.Self.ReportCyclicDependency(state.Declaration, state.Context);
                        return state.Self.CreateErrorSymbol(state.Declaration);
                    }

                    try
                    {
                        return state.Self.ComputeSymbol(state.Declaration, state.Context);
                    }
                    finally
                    {
                        state.Context.ExitInference();
                    }
                });
            },
            (Declaration: declaration, Context: context, Self: this)
        );

        return lazy.Value;
    }

    private VariableSymbol ResolveVariable(
        Name name,
        DeclarationScope scope,
        BindingContext context
    )
    {
        while (true)
        {
            var declarations = scope.FindDeclarations(name);
            var variablesFound = 0;
            VariableSymbol? foundVariable = null;
            foreach (var declaration in declarations)
            {
                if (declaration.Syntax is not VariableDeclarationSyntax)
                    continue;

                foundVariable = (VariableSymbol)GetSymbol(declaration, context);
                variablesFound++;
            }

            if (foundVariable is not null && variablesFound == 1)
                return foundVariable;

            if (variablesFound > 1)
            {
                var names = ImmutableArray.Create<object?>(name);
                foreach (var declaration in declarations)
                {
                    if (declaration.Syntax is not VariableDeclarationSyntax)
                        continue;

                    context.ReportDiagnostic(
                        new Diagnostic
                        {
                            Descriptor = SemanticDiagnostics.DuplicateSymbolDefinition,
                            Range = declaration.Syntax.Range,
                            Arguments = names,
                        }
                    );
                }

                return new VariableSymbol { Name = name, Type = ErrorTypeSymbol.Default };
            }

            if (scope.Parent is null)
                return new VariableSymbol { Name = name, Type = ErrorTypeSymbol.Default };

            scope = scope.Parent;
        }
    }

    public BoundNode BindNode(SyntaxNode node, DeclarationScope scope, CompilationContext context)
    {
        return BindNode(node, scope, new BindingContext(context));
    }

    private BoundNode BindNode(SyntaxNode node, DeclarationScope scope, BindingContext context)
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
        BindingContext context
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
        BindingContext context
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
        BindingContext context
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
        BindingContext context
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

    private Symbol ComputeSymbol(SymbolDeclaration declaration, BindingContext context)
    {
        return declaration.Syntax switch
        {
            VariableDeclarationSyntax variableDeclaration => ResolveVariable(
                declaration,
                variableDeclaration,
                context
            ),
            _ => throw new InvalidOperationException(),
        };
    }

    private VariableSymbol ResolveVariable(
        SymbolDeclaration declaration,
        VariableDeclarationSyntax variable,
        BindingContext context
    )
    {
        var type =
            variable.Type switch
            {
                not null => ResolveType(variable.Type, context),
                null => null,
            } ?? InferVariableType(variable, declaration.EnclosingScope, context);

        return new VariableSymbol
        {
            Name = declaration.Name,
            Type = type,
            Declaration = variable,
            IsMutable = variable.IsMutable,
        };
    }

    private TypeSymbol InferVariableType(
        VariableDeclarationSyntax variable,
        DeclarationScope scope,
        BindingContext context
    )
    {
        TypeSymbol type;
        if (variable.Initializer is null)
        {
            context.ReportDiagnostic(
                new Diagnostic
                {
                    Descriptor = SemanticDiagnostics.VariableWithoutTypeOrInitializer,
                    Range = variable.Range,
                    Arguments = [variable.Identifier.Name],
                }
            );
            type = ErrorTypeSymbol.Default;
        }
        else
        {
            var expression = (BoundExpression)BindNode(variable.Initializer, scope, context);
            type = expression.Type;
        }

        return type;
    }

    // ReSharper disable once UnusedParameter.Local
    private TypeSymbol ResolveType(TypeSyntax typeSyntax, BindingContext context)
    {
        return typeSyntax switch
        {
            BuiltInTypeSyntax builtIn => builtIn.Type.GetSymbol(),
            NamedTypeSyntax namedType => new ErrorTypeSymbol { Name = namedType.Name.Name },
        };
    }

    private void ReportCyclicDependency(SymbolDeclaration declaration, BindingContext context)
    {
        var cycle = context.CollectCycle();
        if (cycle.Length > 0)
        {
            context.ReportDiagnostic(
                new Diagnostic
                {
                    Descriptor = SemanticDiagnostics.CyclicSymbolDefinition,
                    Range = declaration.Syntax.Range,
                    Arguments =
                    [
                        declaration.Name,
                        cycle.AsValueEnumerable().Select(x => x.Name).JoinToString(" -> "),
                    ],
                }
            );
        }
    }

    private Symbol CreateErrorSymbol(SymbolDeclaration declaration)
    {
        throw new NotImplementedException();
    }
}
