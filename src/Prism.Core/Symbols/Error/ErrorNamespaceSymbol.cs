using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorNamespaceSymbol : NamespaceSymbol
{
    internal ErrorNamespaceSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override NamespaceKind NamespaceKind => NamespaceKind.Compilation;
    public override Compilation? ContainingCompilation => null;
}
