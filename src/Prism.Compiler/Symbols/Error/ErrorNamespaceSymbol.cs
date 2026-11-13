using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Error;

internal sealed class ErrorNamespaceSymbol : INamespaceSymbolData
{
    internal ErrorNamespaceSymbol(string name, Symbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
        PublicSymbol = new NamespaceSymbol(this);
    }

    public NamespaceSymbol PublicSymbol { get; }
    public string Name { get; }
    public Symbol? ContainingSymbol { get; }

    public ImmutableArray<Symbol> GetMembers() => [];

    public ImmutableArray<Symbol> GetMembers(string name) => [];

    public NamespaceKind NamespaceKind => NamespaceKind.Assembly;
}
