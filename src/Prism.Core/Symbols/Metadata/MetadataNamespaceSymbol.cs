using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataNamespaceSymbol(
    NamespaceDefinition definition,
    AssemblySymbol containingAssembly,
    NamespaceSymbol? containingSymbol = null
) : NamespaceSymbol
{
    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;

    public override string Name => definition.Name;
    public override Symbol ContainingSymbol => (Symbol?)ContainingNamespace ?? ContainingAssembly;
    public override ImmutableArray<Location> Locations => ContainingAssembly.Locations;
    public override AssemblySymbol ContainingAssembly { get; } = containingAssembly;
    public override NamespaceSymbol? ContainingNamespace { get; } = containingSymbol;
    public override MemberContainerSymbol? ContainingMemberContainer => ContainingNamespace;
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
