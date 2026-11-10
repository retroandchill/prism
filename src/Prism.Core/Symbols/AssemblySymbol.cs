using System.Collections.Immutable;
using Prism.Core.References;
using Prism.Metadata;

namespace Prism.Core.Symbols;

public closed class AssemblySymbol : Symbol, IMetadataReferenceProvider
{
    private readonly AssemblyReferences _references;

    private protected AssemblySymbol(TargetInfo targetInfo, AssemblyReferences? references = null)
    {
        TargetInfo = targetInfo;
        _references = references ?? AssemblyReferences.Empty;
    }

    public sealed override string Name => Identity.Name;

    public sealed override Symbol? ContainingSymbol => null;

    public abstract AssemblyIdentity Identity { get; }

    public TargetInfo TargetInfo { get; }

    public sealed override AssemblySymbol? ContainingAssembly => null;

    public sealed override NamespaceSymbol? ContainingNamespace => null;

    public sealed override MemberContainerSymbol? ContainingMemberContainer => null;

    public sealed override TypeSymbol? ContainingType => null;

    public abstract NamespaceSymbol GlobalNamespace { get; }

    internal ImmutableArray<AssemblyIdentity> ReferencedAssemblies => _references.Identities;

    internal ImmutableArray<AssemblySymbol> ReferencedAssemblySymbols => _references.Symbols;

    internal ImmutableArray<UnifiedAssembly> UnifiedAssemblies => _references.UnifiedAssemblies;

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
    }
}
