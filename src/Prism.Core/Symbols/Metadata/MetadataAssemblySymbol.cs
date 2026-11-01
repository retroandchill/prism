using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol : AssemblySymbol
{
    private readonly AssemblyMetadata _metadata;

    public MetadataAssemblySymbol(AssemblyMetadata metadata)
        : base(metadata.Identity.Name)
    {
        _metadata = metadata;
    }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override AssemblyIdentity Identity => _metadata.Identity;
    public override NamespaceSymbol GlobalNamespace => throw new NotImplementedException();
}
