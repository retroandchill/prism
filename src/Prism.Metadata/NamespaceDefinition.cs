using System.Collections.Immutable;
using System.Diagnostics;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata;

public sealed partial class NamespaceDefinition : MemberContainerDefinition
{
    internal NamespaceDefinition(
        NamespaceDefHandle handle,
        string name,
        NamespaceDefHandle parent,
        MetadataReader reader
    )
        : base(name, reader)
    {
        Handle = handle;
        ParentHandle = parent;
    }

    [MapperConstructor]
    private NamespaceDefinition(NamespaceDefHandle handle, string name)
        : base(name)
    {
        Handle = handle;
    }

    public new NamespaceDefHandle Handle { get; }

    public bool IsGlobal => Handle.IsNone;

    public NamespaceDefHandle ParentHandle { get; private set; }

    public NamespaceDefinition? Parent
    {
        get
        {
            if (ParentHandle.IsNone)
                return null;

            if (field is not null)
                return field;

            Debug.Assert(MetadataReader is not null);
            Interlocked.CompareExchange(ref field, MetadataReader.GetNamespaceDef(Handle), null);
            return field;
        }
        internal set
        {
            field = value;
            ParentHandle = field?.Handle ?? Handle;
        }
    }

    public ImmutableArray<NamespaceDefinition> NestedNamespaces
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(MetadataReader is not null);
            var mappings = MetadataReader.NamespaceDefsByParent[Handle];
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. mappings.Select(x => MetadataReader.GetNamespaceDef(x))]
            );
            return field;
        }
        internal set;
    }

    protected override MemberContainerDefHandle GetHandle() => Handle;

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(MemberContainerDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeDefinition.Mapper))]
    [UseStaticMapper(typeof(VariableDefinition.Mapper))]
    [UseStaticMapper(typeof(FunctionDefinition.Mapper))]
    internal static new partial class Mapper
    {
        [UserMapping(Default = true)]
        public static NamespaceDefinition ToDefinition(
            NamespaceDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        )
        {
            if (handler.TryGetReference(builder, out NamespaceDefinition? existing))
                return existing;

            // Mapperly has a bug where it doesn't like recursive types even with reference handling enabled, so we're using
            // a separate mapper that maps the non-recursive properties and then we manually handle those two.
            var mapped = MapNamespaceWithoutRecursion(builder, handler);
            mapped.Parent = builder.Parent?.ToDefinition(handler);
            mapped.NestedNamespaces =
            [
                .. builder.NestedNamespaces.Select(n => n.ToDefinition(handler)),
            ];
            return mapped;
        }

        [MapperIgnoreSource(nameof(NamespaceDefinitionBuilder.Parent))]
        [MapperIgnoreSource(nameof(NamespaceDefinitionBuilder.NestedNamespaces))]
        [MapperIgnoreTarget(nameof(Parent))]
        [MapperIgnoreTarget(nameof(ParentHandle))]
        [MapperIgnoreTarget(nameof(NestedNamespaces))]
        private static partial NamespaceDefinition MapNamespaceWithoutRecursion(
            NamespaceDefinitionBuilder builder,
            [ReferenceHandler] IReferenceHandler handler
        );
    }
}
