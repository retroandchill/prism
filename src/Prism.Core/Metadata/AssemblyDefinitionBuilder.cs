using System.Collections.ObjectModel;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinitionBuilder(AssemblyIdentity identity, TargetInfo target)
{
    public AssemblyIdentity Identity { get; } = identity;

    public TargetInfo Target { get; } = target;

    private readonly List<AssemblyReference> _referencedAssemblies = [];
    private readonly Dictionary<AssemblyIdentity, AssemblyReference> _referencedAssembliesMap = [];
    private uint _nextAssemblyReferenceId = 2;

    private readonly List<MetadataNamespace> _namespaces = [];
    private uint _nextNamespaceId = 1;

    public AssemblyDefinitionBuilder(AssemblyIdentity identity)
        : this(identity, TargetInfo.CurrentPlatform) { }

    public ReadOnlyCollection<AssemblyReference> ReferencedAssemblies =>
        field ??= _referencedAssemblies.AsReadOnly();

    public ReadOnlyCollection<MetadataNamespace> Namespaces => field ??= _namespaces.AsReadOnly();

    public void AddReferencedAssembly(AssemblyReference reference)
    {
        if (!_referencedAssembliesMap.TryAdd(reference.Identity, reference))
            return;

        _referencedAssemblies.Add(reference);
        reference.Id = _nextAssemblyReferenceId++;
    }

    public MetadataNamespaceId AddNamespace(string name, MetadataNamespaceId parentId = default)
    {
        var id = new MetadataNamespaceId(_nextNamespaceId++);
        _namespaces.Add(new MetadataNamespace(id, parentId, name));
        return id;
    }

    public AssemblyDefinition Build()
    {
        return new AssemblyDefinition(this);
    }
}
