using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol(AssemblyDefinition definition)
    : AssemblySymbol(definition.TargetInfo)
{
    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override AssemblyIdentity Identity => definition.Identity;

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
                new MetadataNamespaceSymbol(definition.GlobalNamespace, this),
                null
            );

            return field;
        }
    }
}
