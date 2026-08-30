using System.Diagnostics;
using Prism.Core.Compiling;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols;

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
}
