using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal class MetadataNamespaceSymbol : NamespaceSymbol
{
    private readonly NamespaceDefinition _definition;

    public MetadataNamespaceSymbol(
        NamespaceDefinition definition,
        AssemblySymbol containingAssembly,
        NamespaceSymbol? containingSymbol = null
    )
        : base(definition.Name, (Symbol?)containingSymbol ?? containingAssembly)
    {
        _definition = definition;
        ContainingAssembly = containingAssembly;
    }

    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;

    public override ImmutableArray<Location> Locations => ContainingAssembly.Locations;
    public override AssemblySymbol ContainingAssembly { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers()
    {
        throw new NotImplementedException();
    }

    public override ImmutableArray<Symbol> GetMembers(string name)
    {
        throw new NotImplementedException();
    }
}
