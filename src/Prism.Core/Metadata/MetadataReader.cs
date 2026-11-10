using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using Prism.Core.Semantic;
using Prism.Utils;

namespace Prism.Core.Metadata;

using AssemblyRefReader = TableRowReader<AssemblyReferenceRow, AssemblyReference>;
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
    private NamespaceDefinition? _globalNamespace;
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

    public MetadataReader(in MetadataLayout layout)
    {
        _stringHeap = new StringHeapReader(layout.StringHeap.Memory, layout.StringHeap.Count);
        _blobHeap = new BlobHeapReader(layout.BlobHeap.Memory, layout.BlobHeap.Count);

        _assemblyRefs = new AssemblyRefReader(layout.AssemblyReferences, CreateAssemblyRef);
        _namespaceDefs = new NamespaceDefReader(layout.NamespaceDefinitions, CreateNamespaceDef);
        _namespaceByParent = _namespaceDefs.RegisterAdditionalWork(
            CreateMemberMap<NamespaceDefRow, NamespaceDefHandle>
        );

        _namespaceRefs = new NamespaceRefReader(layout.NamespaceReferences, CreateNamespaceRef);
        _typeDefs = new TypeDefReader(layout.TypeDefinitions, CreateTypeDef);
        _typeRefs = new TypeRefReader(layout.TypeReferences, CreateTypeRef);
        _typeDefsByParent = _typeDefs.RegisterAdditionalWork(
            CreateMemberMap<TypeDefinitionRow, MemberContainerDefHandle, TypeDefHandle>
        );

        _typeSpecs = new TypeSpecHeapReader(
            layout.TypeSpecHeap.Memory,
            layout.TypeSpecHeap.Count,
            this
        );

        _variables = new VariableReader(layout.Variables, CreateVariable);
        _variablesByParent = _variables.RegisterAdditionalWork(
            CreateMemberMap<VariableRow, MemberContainerDefHandle, VariableHandle>
        );
        _functions = new FunctionReader(layout.Functions, CreateFunction);
        _functionsByParent = _functions.RegisterAdditionalWork(
            CreateMemberMap<FunctionRow, MemberContainerDefHandle, FunctionHandle>
        );
        _parameters = new ParameterReader(layout.Parameters, CreateParameter);
        _parametersByParent = _parameters.RegisterAdditionalWork(
            CreateMemberMap<ParameterRow, FunctionOrAttributeHandle, ParameterHandle>
        );
        _customAttributes = new CustomAttributeReader(
            layout.CustomAttributes,
            CreateCustomAttribute
        );
        _customAttributesByMember = _customAttributes.RegisterAdditionalWork(
            CreateMemberMap<CustomAttributeRow, AttributeHolderHandle, CustomAttributeHandle>
        );
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

    private AssemblyReference CreateAssemblyRef(int rowId, in AssemblyReferenceRow row)
    {
        return new AssemblyReference(
            AssemblyRefHandle.FromRowId(rowId),
            MapAssemblyIdentity(row.Identity)
        );
    }

    public AssemblyIdentity MapAssemblyIdentity(AssemblyIdentityData data)
    {
        var assemblyName = GetString(data.Name);
        var version = data.Version.ToVersion();
        return new AssemblyIdentity(assemblyName, version);
    }

    public NamespaceOrAssemblyReference GetNamespaceOrAssemblyReference(
        NamespaceOrAssemblyRefHandle handle
    )
    {
        return handle switch
        {
            AssemblyRefHandle asm => GetAssemblyRef(asm),
            NamespaceRefHandle ns => GetNamespaceRef(ns),
        };
    }

    public MemberContainerDefinition GetMemberContainerDef(MemberContainerDefHandle handle)
    {
        return handle switch
        {
            NamespaceDefHandle ns => GetNamespaceDef(ns),
            TypeDefHandle type => GetTypeDef(type),
        };
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

    public int NamespaceDefinitionsCount => _namespaceDefs.Count;

    public NamespaceDefinition GetNamespaceDef(NamespaceDefHandle handle)
    {
        if (!handle.IsNone)
            return _namespaceDefs.GetRow(handle.Id);

        if (_globalNamespace is not null)
            return _globalNamespace;

        Interlocked.CompareExchange(
            ref _globalNamespace,
            new NamespaceDefinition(
                NamespaceDefHandle.None,
                CommonNames.Global,
                NamespaceDefHandle.None,
                this
            ),
            null
        );

        return _globalNamespace;
    }

    private readonly RegisteredLazy<NamespaceParentMap> _namespaceByParent;
    public NamespaceParentMap NamespaceDefsByParent => _namespaceByParent.Value;

    private NamespaceDefinition CreateNamespaceDef(int rowId, in NamespaceDefRow row)
    {
        var name = GetString(row.Name);
        return new NamespaceDefinition(NamespaceDefHandle.FromRowId(rowId), name, row.Parent, this);
    }

    public int NamespaceReferenceCount => _namespaceRefs.Count;

    public NamespaceReference GetNamespaceRef(NamespaceRefHandle handle)
    {
        return _namespaceRefs.GetRow(handle.Id);
    }

    private NamespaceReference CreateNamespaceRef(int rowId, in NamespaceRefRow row)
    {
        return new NamespaceReference(
            NamespaceRefHandle.FromRowId(rowId),
            GetString(row.Name),
            row.Parent,
            this
        );
    }

    public int TypeDefinitionsCount => _typeDefs.Count;

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
            row.Container,
            row.Base,
            this
        );
    }

    private readonly RegisteredLazy<TypeContainerMap> _typeDefsByParent;

    public TypeContainerMap TypeDefsByParent => _typeDefsByParent.Value;

    public int TypeReferenceCount => _typeRefs.Count;

    public TypeReference GetTypeRef(TypeRefHandle handle)
    {
        return _typeRefs.GetRow(handle.Id);
    }

    private TypeReference CreateTypeRef(int rowId, in TypeReferenceRow row)
    {
        return new TypeReference(
            TypeRefHandle.FromRowId(rowId),
            GetString(row.Name),
            row.Container,
            this
        );
    }

    public NamedType GetNamedType(NamedTypeHandle handle)
    {
        return handle switch
        {
            TypeDefHandle def => GetTypeDef(def),
            TypeRefHandle @ref => GetTypeRef(@ref),
        };
    }

    public int TypeSpecificationCount => _typeSpecs.Count;

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

    public int VariableCount => _variables.Count;

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
            row.Container,
            row.Type,
            row.ConstantValue,
            this
        );
    }

    public FunctionOrAttribute GetFunctionOrAttribute(FunctionOrAttributeHandle handle)
    {
        return handle switch
        {
            TypeDefHandle type => GetTypeDef(type),
            FunctionHandle function => GetFunction(function),
        };
    }

    public int FunctionCount => _functions.Count;

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
            row.Container,
            row.ReturnType,
            row.ReceiverType,
            this
        );
    }

    private readonly RegisteredLazy<FunctionContainerMap> _functionsByParent;
    public FunctionContainerMap FunctionsByParent => _functionsByParent.Value;

    public int ParameterCount => _parameters.Count;

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
            row.FunctionOrAttribute,
            row.Type,
            row.DefaultValue,
            this
        );
    }

    private readonly RegisteredLazy<ParameterContainerMap> _parametersByParent;
    public ParameterContainerMap ParametersByParent => _parametersByParent.Value;

    public AttributeHolderDefinition GetAttributeHolder(AttributeHolderHandle handle)
    {
        return handle switch
        {
            TypeDefHandle type => GetTypeDef(type),
            VariableHandle variable => GetVariable(variable),
            FunctionHandle function => GetFunction(function),
            ParameterHandle param => GetParameter(param),
        };
    }

    public int CustomAttributeCount => _customAttributes.Count;

    public CustomAttributeDefinition GetCustomAttribute(CustomAttributeHandle handle)
    {
        return _customAttributes.GetRow(handle.Id);
    }

    private CustomAttributeDefinition CreateCustomAttribute(int rowId, in CustomAttributeRow row)
    {
        return new CustomAttributeDefinition(row.Holder, row.Type, row.Arguments, this);
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
