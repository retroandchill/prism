using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Intrinsic;

internal sealed class IntrinsicNamespaceSymbol : INamespaceSymbolData
{
    internal IntrinsicNamespaceSymbol(string name, Symbol containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public string Name { get; }
    public Symbol ContainingSymbol { get; }

    internal ImmutableArray<Symbol> Members
    {
        get;
        set
        {
            if (!field.IsDefault)
                throw new InvalidOperationException("Members cannot be set more than once.");

            field = value;
        }
    }

    public ImmutableArray<Symbol> GetMembers() => Members;

    public ImmutableArray<Symbol> GetMembers(string name)
    {
        return [.. Members.Where(m => m.Name == name)];
    }

    public NamespaceKind NamespaceKind => NamespaceKind.Assembly;
}
