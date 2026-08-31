using System.Collections.Concurrent;
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
        BoundExpression
    > _variableInitializers = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<
        FunctionDeclarationSyntax,
        BoundStatement
    > _functionBodies = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<SyntaxNode, Symbol> _symbols = new(
        ReferenceEqualityComparer.Instance
    );

    public Binder GetBinder(SyntaxNode node)
    {
        throw new NotImplementedException();
    }

    public BoundExpression GetBoundVariableInitializer(
        VariableDeclarationSyntax syntax,
        LookupContext context
    )
    {
        return GetBoundVariableInitializer(syntax, GetBinder(syntax), context);
    }

    public BoundExpression GetBoundVariableInitializer(
        VariableDeclarationSyntax syntax,
        Binder binder,
        LookupContext context
    )
    {
        throw new NotImplementedException();
    }

    public BoundStatement GetBoundFunctionBody(
        FunctionDeclarationSyntax syntax,
        LookupContext context
    )
    {
        throw new NotImplementedException();
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
