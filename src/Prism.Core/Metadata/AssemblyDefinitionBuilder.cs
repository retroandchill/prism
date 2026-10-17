using System.Collections.Immutable;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinitionBuilder(AssemblyIdentity identity, TargetInfo target)
{
    public AssemblyIdentity Identity { get; } = identity;

    public TargetInfo Target { get; } = target;

    public List<AssemblyReference> ReferencedAssemblies { get; } = [];

    public AssemblyDefinitionBuilder(AssemblyIdentity identity)
        : this(identity, TargetInfo.CurrentPlatform) { }
}
