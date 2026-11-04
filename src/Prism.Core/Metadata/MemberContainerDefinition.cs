using System.Collections.Immutable;

namespace Prism.Core.Metadata;

internal closed class MemberContainerDefinition(string name, MetadataReader reader)
{
    public string Name { get; } = name;
    protected MetadataReader MetadataReader { get; } = reader;

    protected abstract MemberContainerDefHandle Handle { get; }

    public ImmutableArray<TypeDefinition> Types
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = MetadataReader.TypeDefsByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(x => MetadataReader.GetTypeDef(x))]
            );
            return field;
        }
    }

    public ImmutableArray<VariableDefinition> Variables
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = MetadataReader.VariablesByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(x => MetadataReader.GetVariable(x))]
            );
            return field;
        }
    }
}
