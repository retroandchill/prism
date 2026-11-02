using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal class MetadataNamespaceSymbol : NamespaceSymbol
{
    private readonly NamespaceDefHandle _namespaceDefHandle;
    private readonly MetadataReader _reader;

    private MetadataNamespaceSymbol(
        string name,
        NamespaceDefHandle namespaceDefHandle,
        AssemblySymbol containingAssembly,
        Symbol? containingSymbol,
        MetadataReader reader
    )
        : base(name, containingSymbol)
    {
        _namespaceDefHandle = namespaceDefHandle;
        ContainingAssembly = containingAssembly;
        _reader = reader;
    }

    public MetadataNamespaceSymbol(
        string name,
        NamespaceDefHandle namespaceDefHandle,
        AssemblySymbol containingAssembly,
        NamespaceSymbol containingSymbol,
        MetadataReader reader
    )
        : this(name, namespaceDefHandle, containingAssembly, (Symbol)containingSymbol, reader)
    {
        ContainingAssembly = containingAssembly;
        _reader = reader;
    }

    public MetadataNamespaceSymbol(
        string name,
        NamespaceDefHandle namespaceDefHandle,
        AssemblySymbol containingAssembly,
        MetadataReader reader
    )
        : this(name, namespaceDefHandle, containingAssembly, containingAssembly, reader)
    {
        ContainingAssembly = containingAssembly;
        _reader = reader;
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
