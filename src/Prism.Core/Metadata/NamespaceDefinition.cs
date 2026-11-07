using System.Collections.Immutable;
using System.Diagnostics;
using Riok.Mapperly.Abstractions;

namespace Prism.Core.Metadata;

public sealed class NamespaceDefinition : MemberContainerDefinition
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
    internal NamespaceDefinition(NamespaceDefHandle handle, string name)
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
}
