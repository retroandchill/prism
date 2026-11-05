using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using MagicArchive;
using Prism.Core.Semantic;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Metadata;

using AssemblyRefReader = TableRowReader<AssemblyIdentityData, AssemblyReference>;
using CustomAttributeMap = FrozenDictionary<
    AttributeHolderHandle,
    ImmutableArray<CustomAttributeHandle>
>;
using CustomAttributeReader = TableRowReader<CustomAttributeRow, CustomAttributeDefinition>;
using FunctionContainerMap = FrozenDictionary<
    MemberContainerDefHandle,
    ImmutableArray<FunctionHandle>
>;
using FunctionReader = TableRowReader<FunctionRow, FunctionDefinition>;
using NamespaceDefReader = TableRowReader<NamespaceDefRow, NamespaceDefinition>;
using NamespaceParentMap = FrozenDictionary<NamespaceDefHandle, ImmutableArray<NamespaceDefHandle>>;
using NamespaceRefReader = TableRowReader<NamespaceRefRow, NamespaceReference>;
using ParameterContainerMap = FrozenDictionary<
    FunctionOrAttributeHandle,
    ImmutableArray<ParameterHandle>
>;
using ParameterReader = TableRowReader<ParameterRow, ParameterDefinition>;
using TypeContainerMap = FrozenDictionary<MemberContainerDefHandle, ImmutableArray<TypeDefHandle>>;
using TypeDefReader = TableRowReader<TypeDefinitionRow, TypeDefinition>;
using TypeRefReader = TableRowReader<TypeReferenceRow, TypeReference>;
using VariableContainerMap = FrozenDictionary<
    MemberContainerDefHandle,
    ImmutableArray<VariableHandle>
>;
using VariableReader = TableRowReader<VariableRow, VariableDefinition>;

internal sealed class MetadataReader
{
    private readonly StringHeapReader _stringHeap;
    private readonly BlobHeapReader _blobHeap;
    private readonly AssemblyRefReader _assemblyRefs;
    private readonly NamespaceDefReader _namespaceDefs;
    private readonly NamespaceRefReader _namespaceRefs;
    private readonly TypeDefReader _typeDefs;
    private readonly TypeRefReader _typeRefs;
    private readonly TypeSpecHeapReader _typeSpecs;
    private readonly VariableReader _variables;
    private readonly FunctionReader _functions;
    private readonly ParameterReader _parameters;
    private readonly CustomAttributeReader _customAttributes;

    private readonly ConcurrentDictionary<ConstantHandle, ConstantValue> _constantsCache = new();

    public MetadataReader(ReadOnlyMemory<byte> buffer)
    {
        using var archiveReaderState = ArchiveReaderStatePool.Rent(
            ArchiveSerializerOptions.LittleEndian
        );
        SectionHeader[]? rentedHeaders = null;
        try
        {
            var reader = new ArchiveReader(buffer.Span, archiveReaderState);
            Span<SectionHeader> sectionHeaders = stackalloc SectionHeader[Sections.Length];
            if (!reader.TryReadInto(sectionHeaders, out var length))
            {
                rentedHeaders = ArrayPool<SectionHeader>.Shared.Rent(length);
                sectionHeaders = rentedHeaders.AsSpan(0, length);
            }
            var read = reader.TryReadInto(sectionHeaders, out _);
            Debug.Assert(read);

            var lookup = sectionHeaders.AsValueEnumerable().ToDictionary(x => x.Kind);
            _stringHeap = CreateHeapReader<StringHeapReader>(
                buffer,
                lookup.GetValueOrDefault(Section.StringTable),
                this
            );
            _blobHeap = CreateHeapReader<BlobHeapReader>(
                buffer,
                lookup.GetValueOrDefault(Section.Blobs),
                this
            );
            _assemblyRefs = CreateTableRowReader<AssemblyIdentityData, AssemblyReference>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.AssemblyReferences),
                CreateAssemblyRef
            );
            _namespaceDefs = CreateTableRowReader<NamespaceDefRow, NamespaceDefinition>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.NamespaceDefs),
                CreateNamespaceDef
            );
            _namespaceByParent = _namespaceDefs.RegisterAdditionalWork(
                CreateMemberMap<NamespaceDefRow, NamespaceDefHandle>
            );

            _namespaceRefs = CreateTableRowReader<NamespaceRefRow, NamespaceReference>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.NamespaceRefs),
                CreateNamespaceRef
            );
            _typeDefs = CreateTableRowReader<TypeDefinitionRow, TypeDefinition>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.TypeDefs),
                CreateTypeDef
            );
            _typeRefs = CreateTableRowReader<TypeReferenceRow, TypeReference>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.TypeRefs),
                CreateTypeRef
            );
            _typeDefsByParent = _typeDefs.RegisterAdditionalWork(
                CreateMemberMap<TypeDefinitionRow, MemberContainerDefHandle, TypeDefHandle>
            );
            _typeSpecs = CreateHeapReader<TypeSpecHeapReader>(
                buffer,
                lookup.GetValueOrDefault(Section.TypeSpecs),
                this
            );
            _variables = CreateTableRowReader<VariableRow, VariableDefinition>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.Variables),
                CreateVariable
            );
            _variablesByParent = _variables.RegisterAdditionalWork(
                CreateMemberMap<VariableRow, MemberContainerDefHandle, VariableHandle>
            );
            _functions = CreateTableRowReader<FunctionRow, FunctionDefinition>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.Functions),
                CreateFunction
            );
            _functionsByParent = _functions.RegisterAdditionalWork(
                CreateMemberMap<FunctionRow, MemberContainerDefHandle, FunctionHandle>
            );
            _parameters = CreateTableRowReader<ParameterRow, ParameterDefinition>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.Parameters),
                CreateParameter
            );
            _parametersByParent = _parameters.RegisterAdditionalWork(
                CreateMemberMap<ParameterRow, FunctionOrAttributeHandle, ParameterHandle>
            );
            _customAttributes = CreateTableRowReader<CustomAttributeRow, CustomAttributeDefinition>(
                buffer,
                archiveReaderState,
                lookup.GetValueOrDefault(Section.CustomAttributes),
                CreateCustomAttribute
            );
            _customAttributesByMember = _customAttributes.RegisterAdditionalWork(
                CreateMemberMap<CustomAttributeRow, AttributeHolderHandle, CustomAttributeHandle>
            );
        }
        finally
        {
            if (rentedHeaders is not null)
                ArrayPool<SectionHeader>.Shared.Return(rentedHeaders);
        }
    }

    public string GetString(StringHandle handle)
    {
        return _stringHeap.GetValue(handle.Id);
    }

    internal T GetBlob<T>(BlobHandle handle)
    {
        return _blobHeap.GetValue<T>(handle.Id);
    }

    public int ReferencedAssembliesCount => _assemblyRefs.Count;

    public AssemblyReference GetAssemblyRef(AssemblyRefHandle handle)
    {
        return _assemblyRefs.GetRow(handle.Id);
    }

    private AssemblyReference CreateAssemblyRef(int rowId, in AssemblyIdentityData data)
    {
        return new AssemblyReference(AssemblyRefHandle.FromRowId(rowId), MapAssemblyIdentity(data));
    }

    public AssemblyIdentity MapAssemblyIdentity(AssemblyIdentityData data)
    {
        var assemblyName = GetString(data.Name);
        var version = data.Version.ToVersion();
        return new AssemblyIdentity(assemblyName, version);
    }

    public MemberContainerReference GetMemberContainerRef(MemberContainerRefHandle handle)
    {
        return handle switch
        {
            AssemblyRefHandle asm => GetAssemblyRef(asm),
            NamespaceRefHandle ns => GetNamespaceRef(ns),
            TypeRefHandle type => GetTypeRef(type),
        };
    }

    public NamespaceDefinition GetNamespaceDef(NamespaceDefHandle handle)
    {
        return _namespaceDefs.GetRow(handle.Id);
    }

    private readonly RegisteredLazy<NamespaceParentMap> _namespaceByParent;
    public NamespaceParentMap NamespaceDefsByParent => _namespaceByParent.Value;

    private NamespaceDefinition CreateNamespaceDef(int rowId, in NamespaceDefRow row)
    {
        var name = GetString(row.Name);
        return new NamespaceDefinition(NamespaceDefHandle.FromRowId(rowId), name, this);
    }

    public NamespaceReference GetNamespaceRef(NamespaceRefHandle handle)
    {
        return _namespaceRefs.GetRow(handle.Id);
    }

    private NamespaceReference CreateNamespaceRef(int rowId, in NamespaceRefRow row)
    {
        return new NamespaceReference(GetString(row.Name), row.Parent, this);
    }

    public TypeDefinition GetTypeDef(TypeDefHandle handle)
    {
        return _typeDefs.GetRow(handle.Id);
    }

    private TypeDefinition CreateTypeDef(int rowId, in TypeDefinitionRow row)
    {
        return new TypeDefinition(
            TypeDefHandle.FromRowId(rowId),
            row.Attributes,
            GetString(row.Name),
            row.Base,
            this
        );
    }

    private readonly RegisteredLazy<TypeContainerMap> _typeDefsByParent;

    public TypeContainerMap TypeDefsByParent => _typeDefsByParent.Value;

    public TypeReference GetTypeRef(TypeRefHandle handle)
    {
        return _typeRefs.GetRow(handle.Id);
    }

    private TypeReference CreateTypeRef(int rowId, in TypeReferenceRow row)
    {
        return new TypeReference(GetString(row.Name), row.Container, this);
    }

    public NamedType GetNamedType(NamedTypeHandle handle)
    {
        return handle switch
        {
            TypeDefHandle def => GetTypeDef(def),
            TypeRefHandle @ref => GetTypeRef(@ref),
        };
    }

    public TypeSpecification GetTypeSpec(TypeSpecHandle handle)
    {
        return _typeSpecs.GetValue(handle.Id);
    }

    public TypeDescriptor GetTypeDescriptor(TypeHandle handle)
    {
        return handle switch
        {
            TypeDefHandle def => GetTypeDef(def),
            TypeRefHandle @ref => GetTypeRef(@ref),
            TypeSpecHandle spec => GetTypeSpec(spec),
        };
    }

    private readonly RegisteredLazy<VariableContainerMap> _variablesByParent;
    public VariableContainerMap VariablesByParent => _variablesByParent.Value;

    public VariableDefinition GetVariable(VariableHandle handle)
    {
        return _variables.GetRow(handle.Id);
    }

    private VariableDefinition CreateVariable(int rowId, in VariableRow row)
    {
        return new VariableDefinition(
            VariableHandle.FromRowId(rowId),
            row.Attributes,
            GetString(row.Name),
            row.Type,
            row.ConstantValue,
            this
        );
    }

    public FunctionDefinition GetFunction(FunctionHandle handle)
    {
        return _functions.GetRow(handle.Id);
    }

    private FunctionDefinition CreateFunction(int rowId, in FunctionRow row)
    {
        return new FunctionDefinition(
            FunctionHandle.FromRowId(rowId),
            row.Attributes,
            GetString(row.Name),
            row.ReturnType,
            row.ReceiverType,
            this
        );
    }

    private readonly RegisteredLazy<FunctionContainerMap> _functionsByParent;
    public FunctionContainerMap FunctionsByParent => _functionsByParent.Value;

    public ParameterDefinition GetParameter(ParameterHandle handle)
    {
        return _parameters.GetRow(handle.Id);
    }

    private ParameterDefinition CreateParameter(int rowId, in ParameterRow row)
    {
        return new ParameterDefinition(
            ParameterHandle.FromRowId(rowId),
            row.Attributes,
            GetString(row.Name),
            row.Type,
            row.DefaultValue,
            this
        );
    }

    private readonly RegisteredLazy<ParameterContainerMap> _parametersByParent;
    public ParameterContainerMap ParametersByParent => _parametersByParent.Value;

    public CustomAttributeDefinition GetCustomAttribute(CustomAttributeHandle handle)
    {
        return _customAttributes.GetRow(handle.Id);
    }

    private CustomAttributeDefinition CreateCustomAttribute(int rowId, in CustomAttributeRow row)
    {
        return new CustomAttributeDefinition(row.Type, row.Arguments, this);
    }

    private readonly RegisteredLazy<CustomAttributeMap> _customAttributesByMember;
    public CustomAttributeMap CustomAttributesByMember => _customAttributesByMember.Value;

    public ConstantValue GetConstant(ConstantHandle handle)
    {
        return _constantsCache.GetOrAdd(
            handle,
            static (handle, assembly) =>
            {
                return handle switch
                {
                    StringHandle str => assembly.GetString(str),
                    BlobHandle blob => assembly.GetBlob<ConstantValue>(blob),
                    InlineConstHandle inline => inline.GetValue(),
                };
            },
            this
        );
    }

    private static T CreateHeapReader<T>(
        ReadOnlyMemory<byte> buffer,
        SectionHeader section,
        MetadataReader reader
    )
        where T : IHeapReader<T>
    {
        return T.Create(buffer.Slice(section.Offset, section.Size), section.Count, reader);
    }

    private static TableRowReader<TRow, T> CreateTableRowReader<TRow, T>(
        ReadOnlyMemory<byte> buffer,
        ArchiveReaderState readerState,
        SectionHeader section,
        ReadMetadataRow<TRow, T> readRow
    )
        where T : class
        where TRow : unmanaged
    {
        return new TableRowReader<TRow, T>(
            buffer.Slice(section.Offset, section.Size),
            readerState,
            section.Count,
            readRow
        );
    }

    private static FrozenDictionary<THandle, ImmutableArray<THandle>> CreateMemberMap<T, THandle>(
        ReadOnlySpan<T> rows
    )
        where T : unmanaged, INestedContainerRow<THandle>
        where THandle : unmanaged, ITableRowHandle<THandle>
    {
        return CreateMemberMap<T, THandle, THandle>(rows);
    }

    private static FrozenDictionary<TContainer, ImmutableArray<THandle>> CreateMemberMap<
        T,
        TContainer,
        THandle
    >(ReadOnlySpan<T> rows)
        where T : unmanaged, INestedContainerRow<TContainer>
        where THandle : unmanaged, ITableRowHandle<THandle>
        where TContainer : unmanaged
    {
        var builder = new Dictionary<TContainer, ImmutableArray<THandle>.Builder>();
        for (var i = 0; i < rows.Length; i++)
        {
            var handle = THandle.Create(i + 1);
            var parent = rows[i].Container;
            var innerBuilder = builder.GetOrAdd(parent, ImmutableArray.CreateBuilder<THandle>);
            innerBuilder.Add(handle);
        }

        return builder.ToFrozenDictionary(t => t.Key, t => t.Value.DrainToImmutable());
    }
}
