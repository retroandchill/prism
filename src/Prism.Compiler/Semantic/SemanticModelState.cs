using System.Collections.Concurrent;
using System.Diagnostics;
using Prism.Compiler.Binding;
using Prism.Compiler.BoundTree;
using Prism.Compiler.Symbols;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Syntax;

namespace Prism.Compiler.Semantic;

internal sealed class SemanticModelState(Compilation compilation, SyntaxTree syntaxTree)
{
    private readonly ConcurrentDictionary<SyntaxNode, Symbol> _symbols = new(
        ReferenceEqualityComparer.Instance
    );

    public Binder GetBinder(SyntaxNode node)
    {
        Debug.Assert(ReferenceEquals(node.SyntaxTree, syntaxTree));
        var factory = compilation.GetBinderFactory(syntaxTree);
        return factory.GetBinder(node);
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
