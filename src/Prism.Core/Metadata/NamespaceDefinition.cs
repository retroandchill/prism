using System.Collections.Immutable;

namespace Prism.Core.Metadata;

internal sealed class NamespaceDefinition(
    NamespaceDefHandle handle,
    string name,
    AssemblyDefinition assembly
) : MemberContainerDefinition(name, assembly)
{
    protected override MemberContainerDefHandle Handle => handle;

    public bool IsGlobal => handle.IsNone;

    public ImmutableArray<NamespaceDefinition> NestedNamespaces
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = Assembly.NamespaceDefsByParent[handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(x => Assembly.GetNamespace(x))]
            );
            return field;
        }
    }
}
