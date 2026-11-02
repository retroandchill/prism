using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol : AssemblySymbol
{
    private readonly MetadataReader _metadataReader;

    public MetadataAssemblySymbol(
        AssemblyIdentity identity,
        TargetInfo targetInfo,
        MetadataReader reader
    )
        : base(identity.Name, targetInfo)
    {
        _metadataReader = reader;
        Identity = identity;
    }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override AssemblyIdentity Identity { get; }
    public override NamespaceSymbol GlobalNamespace => throw new NotImplementedException();
}
