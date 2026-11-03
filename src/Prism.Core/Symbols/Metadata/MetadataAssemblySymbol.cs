using System.Collections.Concurrent;
using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;
using Prism.Core.Utils;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol : AssemblySymbol
{
    private readonly MetadataReader _metadataReader;
    private readonly ConcurrentDictionary<NamespaceDefHandle, NamespaceSymbol> _namespaceDefs =
        new();

    public MetadataAssemblySymbol(
        AssemblyIdentity identity,
        TargetInfo targetInfo,
        MetadataReader reader
    )
        : base(identity.Name, targetInfo)
    {
        _metadataReader = reader;
        Identity = identity;
    }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override AssemblyIdentity Identity { get; }

    public override NamespaceSymbol GlobalNamespace
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            Interlocked.CompareExchange(
                ref field,
                new MetadataNamespaceSymbol(NamespaceDefHandle.None, CommonNames.Global, this),
                null
            );

            return field;
        }
    }

    public MemberContainerSymbol GetMemberContainer(MemberContainerDefHandle handle)
    {
        return handle switch
        {
            TypeDefHandle type => GetType(type),
            NamespaceDefHandle ns => GetNamespace(ns),
        };
    }

    public TypeSymbol GetType(TypeDefHandle typeDefHandle)
    {
        throw new NotImplementedException();
    }

    public NamespaceSymbol GetNamespace(NamespaceDefHandle handle)
    {
        if (handle.IsNone)
            return GlobalNamespace;

        return _namespaceDefs.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                ref readonly var row = ref assembly._metadataReader.GetNamespaceDef(handle);
                var parent = assembly.GetNamespace(row.Parent);
                var name = assembly._metadataReader.GetString(row.Name);
                return new MetadataNamespaceSymbol(handle, name, assembly, parent);
            },
            this
        );
    }
}
