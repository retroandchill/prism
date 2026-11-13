using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols;

internal static class SymbolHelpers
{
    public static void CacheSymbol(this Compilation compilation, SyntaxNode node, Symbol symbol)
    {
        var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);
        semanticModel.State.CacheSymbol(symbol, node);
    }

    public static SyntaxNode GetNamespaceDeclarationSyntax(SyntaxReference reference)
    {
        var node = reference.Syntax;
        while (node is NameSyntax nameSyntax)
        {
            node = node.Parent;
        }

        Debug.Assert(node is CompilationUnitSyntax or NamespaceDeclarationSyntax);
        return node;
    }

    extension(Symbol symbol)
    {
        internal LexicalSortKey GetLexicalSortKey()
        {
            var firstLocation = symbol.TryGetFirstLocation();
            if (firstLocation is null)
                return LexicalSortKey.NotInSource;

            var declaringCompilation = symbol.DeclaringCompilation;
            Debug.Assert(declaringCompilation is not null);
            return new LexicalSortKey((SourceLocation)firstLocation, declaringCompilation);
        }

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences =>
            symbol.Data is ICompiledSymbolData compiledSymbol
                ? compiledSymbol.DeclaringSyntaxReferences
                : [];

        internal Compilation? DeclaringCompilation =>
            symbol.Data is ICompiledSymbolData compiledSymbol
                ? compiledSymbol.DeclaringCompilation
                : null;
    }

    extension(NamespaceSymbol ns)
    {
        public Compilation? ContainingCompilation =>
            ns.Data is MergedNamespaceSymbol mergedNamespace
                ? mergedNamespace.ContainingCompilation
                : null;
    }
}
