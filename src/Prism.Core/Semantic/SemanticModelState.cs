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
