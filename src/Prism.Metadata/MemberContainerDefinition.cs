using System.Collections.Immutable;
using System.Diagnostics;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata;

public closed partial class MemberContainerDefinition
{
    private protected MemberContainerDefinition(string name, MetadataReader? reader = null)
    {
        Name = name;
        MetadataReader = reader;
    }

    public string Name { get; }
    private protected MetadataReader? MetadataReader { get; }

    public MemberContainerDefHandle Handle => GetHandle();

    public ImmutableArray<TypeDefinition> Types
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.TypeDefsByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetTypeDef)]
            );
            return field;
        }
        internal set;
    }

    public ImmutableArray<VariableDefinition> Variables
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.VariablesByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetVariable)]
            );
            return field;
        }
        internal set;
    }

    public ImmutableArray<FunctionDefinition> Functions
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.FunctionsByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(MetadataReader.GetFunction)]
            );
            return field;
        }
        internal set;
    }

    protected abstract MemberContainerDefHandle GetHandle();

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(NamespaceDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeDefinition.Mapper))]
    internal static partial class Mapper
    {
        [MapDerivedType<NamespaceDefinitionBuilder, NamespaceDefinition>]
        [MapDerivedType<TypeDefinitionBuilder, TypeDefinition>]
        public static partial MemberContainerDefinition ToDefinition(
            MemberContainerDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
