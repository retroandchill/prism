using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagicArchive;
using Prism.Core.Semantic;
using Prism.Core.Utils;

namespace Prism.Core.Metadata;

internal sealed class AssemblyDefinition : AssemblyDescriptor
{
    public TargetInfo TargetInfo { get; }

    private readonly MetadataReader _metadataReader;

    private readonly ConcurrentDictionary<AssemblyRefHandle, AssemblyReference> _referenceCache =
        new();
    private readonly ConcurrentDictionary<
        NamespaceDefHandle,
        NamespaceDefinition
    > _namespaceDefCache = new();
    private readonly ConcurrentDictionary<
        NamespaceRefHandle,
        NamespaceReference
    > _namespaceRefCache = new();

    private readonly ConcurrentDictionary<TypeDefHandle, TypeDefinition> _typeDefCache = new();
    private readonly ConcurrentDictionary<TypeRefHandle, TypeReference> _typeRefCache = new();
    private readonly ConcurrentDictionary<TypeSpecHandle, TypeSpecification> _typeSpecCache = new();

    private readonly ConcurrentDictionary<VariableHandle, VariableDefinition> _variablesCache =
        new();

    private readonly ConcurrentDictionary<ConstantHandle, ConstantValue> _constantsCache = new();

    private AssemblyDefinition(
        AssemblyIdentity identity,
        TargetInfo targetInfo,
        MetadataReader reader
    )
        : base(identity)
    {
        TargetInfo = targetInfo;
        _metadataReader = reader;
    }

    public static AssemblyDefinition Load(string filepath)
    {
        byte[] buffer;
        using (var stream = File.OpenRead(filepath))
        {
            Debug.Assert(stream.CanSeek);
            buffer = new byte[stream.Length];
            stream.ReadExactly(buffer);
        }

        using var readerState = ArchiveReaderStatePool.Rent(ArchiveSerializerOptions.LittleEndian);
        var reader = new ArchiveReader(buffer, readerState);

        var (identityData, targetInfo) = reader.ReadArchivable<MetadataHeader>();
        var remaining = buffer.AsMemory(reader.ConsumedBytes);
        var metadataReader = new MetadataReader(remaining);

        var identity = MapAssemblyIdentity(identityData, metadataReader);
        return new AssemblyDefinition(identity, targetInfo, metadataReader);
    }

    private static AssemblyIdentity MapAssemblyIdentity(
        AssemblyIdentityData data,
        MetadataReader metadataReader
    )
    {
        var assemblyName = metadataReader.GetString(data.Name);
        var version = data.Version.ToVersion();
        return new AssemblyIdentity(assemblyName, version);
    }

    private AssemblyReference GetAssemblyRef(AssemblyRefHandle handle)
    {
        return _referenceCache.GetOrAdd(
            handle,
            static (handle, metadataReader) =>
            {
                ref readonly var data = ref metadataReader.GetAssemblyRef(handle);
                return new AssemblyReference(handle, MapAssemblyIdentity(data, metadataReader));
            },
            _metadataReader
        );
    }

    public ImmutableArray<AssemblyReference> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            var referenced = new AssemblyReference[_metadataReader.ReferencedAssembliesCount];
            for (var i = 0; i < referenced.Length; i++)
            {
                referenced[i] = GetAssemblyRef(AssemblyRefHandle.FromRowId(i + 1));
            }

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ImmutableCollectionsMarshal.AsImmutableArray(referenced)
            );

            return field;
        }
    }

    public NamespaceDefinition GlobalNamespace
    {
        get
        {
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                new NamespaceDefinition(NamespaceDefHandle.None, CommonNames.Global, this),
                null
            );
            return field;
        }
    }

    public MemberContainerDefinition GetMemberContainer(MemberContainerDefHandle handle)
    {
        return handle switch
        {
            NamespaceDefHandle namespaceDefHandle => GetNamespace(namespaceDefHandle),
            TypeDefHandle typeDefHandle => GetType(typeDefHandle),
            null => GlobalNamespace,
        };
    }

    public MemberContainerReference GetMemberContainer(MemberContainerRefHandle handle)
    {
        return handle switch
        {
            NamespaceRefHandle namespaceRefHandle => GetNamespace(namespaceRefHandle),
            TypeRefHandle typeRefHandle => GetType(typeRefHandle),
            AssemblyRefHandle assemblyRefHandle => GetAssemblyRef(assemblyRefHandle),
        };
    }

    public NamespaceDefinition GetNamespace(NamespaceDefHandle handle)
    {
        if (handle.IsNone)
            return GlobalNamespace;

        return _namespaceDefCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                ref readonly var row = ref assembly._metadataReader.GetNamespaceDef(handle);
                var name = assembly._metadataReader.GetString(row.Name);
                return new NamespaceDefinition(handle, name, assembly);
            },
            this
        );
    }

    public NamespaceReference GetNamespace(NamespaceRefHandle handle)
    {
        return _namespaceRefCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                ref readonly var row = ref assembly._metadataReader.GetNamespaceRef(handle);
                var name = assembly._metadataReader.GetString(row.Name);
                var owning = assembly.GetNamespaceOrAssembly(row.Parent);
                return owning! switch
                {
                    AssemblyReference assemblyReference => new NamespaceReference(
                        name,
                        assemblyReference
                    ),
                    NamespaceReference namespaceReference => new NamespaceReference(
                        name,
                        namespaceReference
                    ),
                };
            },
            this
        );
    }

    public NamespaceOrAssemblyReference GetNamespaceOrAssembly(NamespaceOrAssemblyRefHandle handle)
    {
        return handle switch
        {
            AssemblyRefHandle h => GetAssemblyRef(h),
            NamespaceRefHandle h => GetNamespace(h),
        };
    }

    public FrozenDictionary<
        NamespaceDefHandle,
        ImmutableArray<NamespaceDefHandle>
    > NamespaceDefsByParent
    {
        get
        {
            if (field is not null)
                return field;

            var builder =
                new Dictionary<NamespaceDefHandle, ImmutableArray<NamespaceDefHandle>.Builder>();
            for (var i = 0; i < _metadataReader.NamespaceDefsCount; i++)
            {
                var handle = NamespaceDefHandle.FromRowId(i + 1);
                var parent = _metadataReader.GetNamespaceDef(handle).Parent;
                var innerBuilder = builder.GetOrAdd(
                    parent,
                    ImmutableArray.CreateBuilder<NamespaceDefHandle>
                );
                innerBuilder.Add(handle);
            }

            Interlocked.CompareExchange(
                ref field,
                builder.ToFrozenDictionary(t => t.Key, t => t.Value.DrainToImmutable()),
                null
            );
            return field;
        }
    }

    public TypeDefinition GetType(TypeDefHandle handle)
    {
        return _typeDefCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                ref readonly var row = ref assembly._metadataReader.GetTypeDef(handle);
                var name = assembly._metadataReader.GetString(row.Name);
                return new TypeDefinition(handle, name, assembly);
            },
            this
        );
    }

    public NamedType? GetBaseType(TypeDefHandle handle)
    {
        return GetType(_metadataReader.GetTypeDef(handle).Base);
    }

    public TypeReference GetType(TypeRefHandle handle)
    {
        return _typeRefCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                ref readonly var row = ref assembly._metadataReader.GetTypeRef(handle);
                var name = assembly._metadataReader.GetString(row.Name);
                var owner = assembly.GetMemberContainer(row.Container);
                return new TypeReference(name, owner);
            },
            this
        );
    }

    public NamedType? GetType(NamedTypeHandle handle)
    {
        return handle switch
        {
            TypeDefHandle typeDef => GetType(typeDef),
            TypeRefHandle typeRef => GetType(typeRef),
            null => null,
        };
    }

    public TypeSpecification GetType(TypeSpecHandle handle)
    {
        return _typeSpecCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                var row = assembly._metadataReader.GetTypeSpec(handle);
                return row switch
                {
                    NullableTypeRow(var element) => new NullableTypeSpecification(
                        assembly.GetType(element)
                    ),
                    ReferenceTypeRow(var element, var mutable) => new ReferenceTypeSpecification(
                        assembly.GetType(element),
                        mutable
                    ),
                    ArrayTypeRow(var element, var length) => new ArrayTypeSpecification(
                        assembly.GetType(element),
                        length
                    ),
                };
            },
            this
        );
    }

    public TypeDescriptor GetType(TypeHandle handle)
    {
        return handle switch
        {
            TypeDefHandle def => GetType(def),
            TypeRefHandle reference => GetType(reference),
            TypeSpecHandle spec => GetType(spec),
        };
    }

    public FrozenDictionary<
        MemberContainerDefHandle,
        ImmutableArray<TypeDefHandle>
    > TypeDefsByParent
    {
        get
        {
            if (field is not null)
                return field;

            var builder =
                new Dictionary<MemberContainerDefHandle, ImmutableArray<TypeDefHandle>.Builder>();
            for (var i = 0; i < _metadataReader.TypeDefsCount; i++)
            {
                var handle = TypeDefHandle.FromRowId(i + 1);
                var parent = _metadataReader.GetTypeDef(handle).Container;
                var innerBuilder = builder.GetOrAdd(
                    parent,
                    ImmutableArray.CreateBuilder<TypeDefHandle>
                );
                innerBuilder.Add(handle);
            }

            Interlocked.CompareExchange(
                ref field,
                builder.ToFrozenDictionary(t => t.Key, t => t.Value.DrainToImmutable()),
                null
            );
            return field;
        }
    }

    public VariableDefinition GetVariable(VariableHandle handle)
    {
        return _variablesCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                ref readonly var row = ref assembly._metadataReader.GetVariable(handle);
                var name = assembly._metadataReader.GetString(row.Name);
                return new VariableDefinition(handle, row.Attributes, name, assembly);
            },
            this
        );
    }

    public TypeDescriptor GetVariableType(VariableHandle handle)
    {
        return GetType(_metadataReader.GetVariable(handle).Type);
    }

    public ConstantValue? GetVariableConstantValue(VariableHandle handle)
    {
        var constantHandle = _metadataReader.GetVariable(handle).ConstantValue;
        if (constantHandle.IsNone)
            return null;

        return GetConstant(constantHandle);
    }

    public ConstantValue GetConstant(ConstantHandle handle)
    {
        return _constantsCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                return handle switch
                {
                    StringHandle str => assembly._metadataReader.GetString(str),
                    BlobHandle blob => assembly._metadataReader.GetBlob<ConstantValue>(blob),
                    InlineConstHandle inline => inline.GetValue(),
                };
            },
            this
        );
    }
}
