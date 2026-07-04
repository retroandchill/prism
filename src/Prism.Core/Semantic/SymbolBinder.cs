// @file SymbolBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Prism.Core.Ast;
using Prism.Core.Strings;
using ZLinq;

namespace Prism.Core.Semantic;

public sealed class SymbolBinder
{
    private readonly ConcurrentDictionary<DeclarationSyntax, Lazy<Symbol>> _symbolCache = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly ConcurrentDictionary<SymbolDeclaration, TypeSymbol> _symbolTypes = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly ConcurrentDictionary<SyntaxNode, Symbol> _typeCache = new(
        ReferenceEqualityComparer.Instance
    );

    public Symbol GetSymbol(SymbolDeclaration declaration, CompilationContext context) =>
        GetSymbol(declaration, new BindingContext(context));

    internal Symbol GetSymbol(SymbolDeclaration declaration, BindingContext context)
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

    private TypeSymbol GetEvaluatedType(SymbolDeclaration declaration, BindingContext context)
    {
        return _symbolTypes.GetOrAdd(
            declaration,
            (decl, state) =>
            {
                var symbol = state.Self.GetSymbol(decl, state.Context);
                return symbol switch
                {
                    VariableSymbol variable => variable.Type,
                    FunctionSymbol function => function.ReturnType,
                    ParameterSymbol parameter => parameter.Type,
                };
            },
            (Context: context, Self: this)
        );
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
        var type = variable.Type switch
        {
            not null => ResolveType(variable.Type),
            null => null,
        };
        if (type is not null)
        {
            _symbolTypes[declaration] = type;
        }

        throw new NotImplementedException();
    }

    private static TypeSymbol ResolveType(TypeSyntax typeSyntax)
    {
        return typeSyntax switch
        {
            BuiltInTypeSyntax builtIn => builtIn.Type.GetSymbol(),
            NamedTypeSyntax => ErrorTypeSymbol.Instance,
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
