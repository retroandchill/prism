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
                [.. mappings.Select(MetadataReader.GetTypeDef)]
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
                [.. mappings.Select(MetadataReader.GetVariable)]
            );
            return field;
        }
    }

    public ImmutableArray<FunctionDefinition> Functions
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var mappings = MetadataReader.FunctionsByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetFunction)]
            );
            return field;
        }
    }
}
