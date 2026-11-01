using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Symbols;

namespace Prism.Core.References;

internal readonly struct UnifiedAssembly
{
    public AssemblyIdentity OriginalReference { get; }

    public AssemblySymbol TargetAssembly { get; }

    public UnifiedAssembly(AssemblyIdentity originalReference, AssemblySymbol targetAssembly)
    {
        Debug.Assert(originalReference is not null);
        Debug.Assert(targetAssembly is not null);

        OriginalReference = originalReference;
        TargetAssembly = targetAssembly;
    }
}

internal sealed class AssemblyReferences
{
    public ImmutableArray<AssemblyIdentity> Identities { get; }

    public ImmutableArray<AssemblySymbol> Symbols { get; }

    public ImmutableArray<UnifiedAssembly> UnifiedAssemblies { get; }

    public AssemblyReferences(
        ImmutableArray<AssemblyIdentity> identities,
        ImmutableArray<AssemblySymbol> symbols,
        ImmutableArray<UnifiedAssembly> unifiedAssemblies
    )
    {
        Debug.Assert(!identities.IsDefault);
        Debug.Assert(!symbols.IsDefault);
        Debug.Assert(identities.Length == symbols.Length);
        Debug.Assert(!unifiedAssemblies.IsDefault);

        Identities = identities;
        Symbols = symbols;
        UnifiedAssemblies = unifiedAssemblies;
    }
}
