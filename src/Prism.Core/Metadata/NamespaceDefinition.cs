using System.Collections.Immutable;

namespace Prism.Core.Metadata;

internal sealed class NamespaceDefinition(
    NamespaceDefHandle handle,
    string name,
    MetadataReader reader
) : MemberContainerDefinition(name, reader)
{
    protected override MemberContainerDefHandle Handle => handle;

    public bool IsGlobal => handle.IsNone;

    public ImmutableArray<NamespaceDefinition> NestedNamespaces
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = MetadataReader.NamespaceDefsByParent[handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(x => MetadataReader.GetNamespaceDef(x))]
            );
            return field;
        }
    }
}
