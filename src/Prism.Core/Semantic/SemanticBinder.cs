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
            LiteralExpressionSyntax literal => BindLiteralExpression(literal, context),
            IdentifierExpressionSyntax identifier => BindIdentifierExpression(
                identifier,
                scope,
                context
            ),
            _ => throw new NotImplementedException(),
        };
    }

    private BoundLiteralExpression BindLiteralExpression(
        LiteralExpressionSyntax literal,
        BindingContext context
    )
    {
        var span = context.GetSpan(literal.Range);
        TypeSymbol boundType;
        LiteralValue value;
        var overflow = false;
        switch (literal.Kind)
        {
            case LiteralKind.BoolTrue:
                boundType = BuiltInTypes.Bool;
                value = true;
                break;
            case LiteralKind.BoolFalse:
                boundType = BuiltInTypes.Bool;
                value = false;
                break;
            case LiteralKind.Integer:
            {
                (boundType, var suffixLength) = Numerics.GetIntegerTypeFromSuffix(span);
                (value, overflow) = Numerics.ParseInteger(span[..^suffixLength]);
                break;
            }
            case LiteralKind.Float:
            {
                (boundType, var suffixLength) = Numerics.GetFloatTypeFromSuffix(span);
                (value, overflow) = Numerics.ParseFloat(span[..^suffixLength]);
                break;
            }
            case LiteralKind.String:
                boundType = BuiltInTypes.Str;
                value = span[1..^1].ToString();
                break;
            default:
                throw new InvalidOperationException();
        }

        if (overflow)
        {
            context.ReportDiagnostic(
                new Diagnostic
                {
                    Descriptor = SemanticDiagnostics.LiteralExceedsMaximumMagnitude,
                    Range = literal.Range,
                }
            );
        }

        return new BoundLiteralExpression { Type = boundType, Value = value };
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
