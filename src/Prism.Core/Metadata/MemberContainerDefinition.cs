using System.Collections.Immutable;

namespace Prism.Core.Metadata;

internal closed class MemberContainerDefinition(string name, AssemblyDefinition assembly)
{
    public string Name { get; } = name;
    protected AssemblyDefinition Assembly { get; } = assembly;

    protected abstract MemberContainerDefHandle Handle { get; }

    public ImmutableArray<TypeDefinition> Types
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = Assembly.TypeDefsByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(x => Assembly.GetType(x))]
            );
            return field;
        }
    }
}
