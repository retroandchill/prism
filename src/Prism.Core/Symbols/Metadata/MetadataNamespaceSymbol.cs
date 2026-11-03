using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal class MetadataNamespaceSymbol : NamespaceSymbol
{
    private readonly NamespaceDefHandle _namespaceDefHandle;
    private readonly MetadataAssemblySymbol _containingAssembly;

    public MetadataNamespaceSymbol(
        NamespaceDefHandle namespaceDefHandle,
        string name,
        MetadataAssemblySymbol containingAssembly,
        NamespaceSymbol? containingSymbol = null
    )
        : base(name, (Symbol?)containingSymbol ?? containingAssembly)
    {
        _namespaceDefHandle = namespaceDefHandle;
        _containingAssembly = containingAssembly;
    }

    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;

    public override ImmutableArray<Location> Locations => _containingAssembly.Locations;
    public override AssemblySymbol ContainingAssembly => _containingAssembly;
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
