using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;
using Prism.Core.Utils;

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

    public override NamespaceSymbol GlobalNamespace
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            Interlocked.CompareExchange(
                ref field,
                new MetadataNamespaceSymbol(
                    CommonNames.Global,
                    NamespaceDefHandle.None,
                    this,
                    _metadataReader
                ),
                null
            );

            return field;
        }
    }
}
