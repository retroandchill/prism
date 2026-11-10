using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Diagnostics;
using Prism.Syntax;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorNamespaceSymbol : NamespaceSymbol
{
    internal ErrorNamespaceSymbol(string name, Symbol? containingSymbol)
    {
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    public override string Name { get; }
    public override Symbol? ContainingSymbol { get; }

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override NamespaceKind NamespaceKind => NamespaceKind.Compilation;
    public override Compilation? ContainingCompilation => null;
}
