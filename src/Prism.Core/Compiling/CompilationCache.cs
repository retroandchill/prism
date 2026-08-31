using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Binding;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Compiling;

internal sealed class CompilationCache(Compilation compilation)
{
    private readonly record struct SymbolLookupKey(Symbol? Containing, string Name);

    private readonly ConcurrentDictionary<
        NamespaceSymbol,
        NamespaceSymbol?
    > _compilationNamespaces = new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<SyntaxTree, BinderFactory> _binderFactories = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<SyntaxTree, SemanticModel> _semanticModels = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<SymbolLookupKey, NamedTypeSymbol> _errorTypes = new();
    private readonly ConcurrentDictionary<SymbolLookupKey, NamespaceSymbol> _errorNamespaces =
        new();

    public NamespaceSymbol? GetCompilationNamespace(NamespaceSymbol symbol)
    {
        if (
            symbol.NamespaceKind == NamespaceKind.Compilation
            && symbol.ContainingCompilation == compilation
        )
            return symbol;

        return _compilationNamespaces.GetOrAdd(
            symbol,
            static (s, self) => self.ComputeCompilationNamespace(s),
            this
        );
    }

    private NamespaceSymbol? ComputeCompilationNamespace(NamespaceSymbol symbol)
    {
        var containingNamespace = symbol.ContainingNamespace;
        if (containingNamespace is null)
            return compilation.GlobalNamespace;

        var current = GetCompilationNamespace(containingNamespace);
        if (current is null)
            return null;

        var found = current.GetNestedNamespace(symbol.Name);
        Debug.Assert(found is not null);
        return found;
    }

    public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }

    public NamedTypeSymbol CreateErrorTypeSymbol(Symbol? symbol, string name)
    {
        throw new NotImplementedException();
    }

    public NamespaceSymbol CreateErrorNamespaceSymbol(NamespaceSymbol? symbol, string name)
    {
        throw new NotImplementedException();
    }

    public BinderFactory GetBinderFactory(SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }

    public Binder RootBinder
    {
        get { throw new NotImplementedException(); }
    }

    public ImmutableArray<VariableSymbol> GetGlobalVariables()
    {
        throw new NotImplementedException();
    }

    public ImmutableArray<FunctionSymbol> GetGlobalFunctions()
    {
        throw new NotImplementedException();
    }
}
