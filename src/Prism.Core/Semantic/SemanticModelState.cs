using System.Collections.Concurrent;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic;

internal sealed class SemanticModelState(Compilation compilation, SyntaxTree syntaxTree)
{
    private readonly ConcurrentDictionary<
        VariableDeclarationSyntax,
        Lazy<BoundExpression>
    > _variableInitializers = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<
        FunctionDeclarationSyntax,
        Lazy<BoundStatement>
    > _functionBodies = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<SyntaxNode, Symbol> _symbols = new(
        ReferenceEqualityComparer.Instance
    );

    public Binder GetBinder(SyntaxNode node)
    {
        Debug.Assert(ReferenceEquals(node.SyntaxTree, syntaxTree));
        var factory = compilation.GetBinderFactory(syntaxTree);
        return factory.GetBinder(node);
    }

    public BoundExpression GetBoundVariableInitializer(
        VariableDeclarationSyntax declaration,
        LookupContext context
    )
    {
        return GetBoundVariableInitializer(declaration, GetBinder(declaration), context);
    }

    public BoundExpression GetBoundVariableInitializer(
        VariableDeclarationSyntax declaration,
        Binder binder,
        LookupContext context
    )
    {
        Debug.Assert(declaration.Initializer is not null);
        var lazy = _variableInitializers.GetOrAdd(
            declaration,
            _ => new Lazy<BoundExpression>(
                () => binder.BindExpression(declaration.Initializer.Value, context),
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );
        return lazy.Value;
    }

    public BoundStatement GetBoundFunctionBody(
        FunctionDeclarationSyntax declaration,
        LookupContext context
    )
    {
        var symbol = GetDeclaredSymbol(declaration) as FunctionSymbol;
        Debug.Assert(symbol is not null);
        var lazy = _functionBodies.GetOrAdd(
            declaration,
            _ => new Lazy<BoundStatement>(
                () =>
                {
                    if (declaration.Body is not null)
                    {
                        Debug.Assert(declaration.ExpressionBody is null);
                        var binder = GetBinder(declaration.Body);
                        return binder.BindStatement(declaration.Body, symbol.ReturnType, context);
                    }
                    else
                    {
                        Debug.Assert(declaration.ExpressionBody is not null);
                        var binder = GetBinder(declaration.ExpressionBody);
                        return binder.BindExpressionBody(
                            declaration.ExpressionBody,
                            symbol.ReturnType,
                            context
                        );
                    }
                },
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );
        return lazy.Value;
    }

    public Symbol? GetDeclaredSymbol(SyntaxNode syntax)
    {
        return _symbols.GetValueOrDefault(syntax);
    }

    public void CacheSymbol(Symbol symbol, SyntaxNode syntax)
    {
        if (!_symbols.TryAdd(syntax, symbol))
        {
            throw new InvalidOperationException("Symbol already cached");
        }
    }
}
