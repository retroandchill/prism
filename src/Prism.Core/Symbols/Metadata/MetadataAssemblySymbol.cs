using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol : AssemblySymbol
{
    private readonly AssemblyDefinition _definition;

    public MetadataAssemblySymbol(AssemblyDefinition definition)
        : base(definition.Identity.Name, definition.TargetInfo)
    {
        _definition = definition;
    }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override AssemblyIdentity Identity => _definition.Identity;

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
                new MetadataNamespaceSymbol(_definition.GlobalNamespace, this),
                null
            );

            return field;
        }
    }
}
