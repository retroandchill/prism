using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MagicArchive;
using Microsoft.Extensions.FileProviders;
using Riok.Mapperly.Abstractions;

namespace Prism.Metadata;

public sealed partial class AssemblyDefinition
{
    public Prism.AssemblyIdentity Identity { get; }
    public Configuration.TargetInfo TargetInfo { get; }

    private readonly MetadataReader? _metadataReader;

    private AssemblyDefinition(in MetadataLayout layout)
    {
        _metadataReader = new MetadataReader(in layout);
        Identity = _metadataReader.MapAssemblyIdentity(layout.Header.AssemblyIdentity);
        TargetInfo = layout.Header.TargetInfo;
    }

    [MapperConstructor]
    internal AssemblyDefinition(
        Prism.AssemblyIdentity identity,
        Configuration.TargetInfo targetInfo
    )
    {
        Identity = identity;
        TargetInfo = targetInfo;
    }

    public ImmutableArray<AssemblyReference> ReferencedAssemblies
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.ReferencedAssembliesCount, static (r, i) =>
                    r.GetAssemblyRef(AssemblyRefHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    public NamespaceDefinition GlobalNamespace
    {
        get
        {
            if (field is not null)
                return field;

            Debug.Assert(_metadataReader is not null);
            Interlocked.CompareExchange(
                ref field,
                _metadataReader.GetNamespaceDef(NamespaceDefHandle.None),
                null
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<NamespaceDefinition> DefinedNamespaces
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.NamespaceDefinitionsCount, static (r, i) =>
                    r.GetNamespaceDef(NamespaceDefHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<NamespaceReference> ReferencedNamespaces
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.NamespaceReferenceCount, static (r, i) =>
                    r.GetNamespaceRef(NamespaceRefHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<TypeDefinition> DefinedTypes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.TypeDefinitionsCount, static (r, i) =>
                    r.GetTypeDef(TypeDefHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<TypeReference> ReferencedTypes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.TypeReferenceCount, static (r, i) =>
                    r.GetTypeRef(TypeRefHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<TypeSpecification> TypeSpecifications
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.TypeSpecificationCount, static (r, i) =>
                    r.GetTypeSpec(TypeSpecHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<VariableDefinition> Variables
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.VariableCount, static (r, i) =>
                    r.GetVariable(VariableHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<FunctionDefinition> Functions
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.FunctionCount, static (r, i) =>
                    r.GetFunction(FunctionHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<ParameterDefinition> Parameters
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.ParameterCount, static (r, i) =>
                    r.GetParameter(ParameterHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    internal ImmutableArray<CustomAttributeDefinition> CustomAttributes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            Debug.Assert(_metadataReader is not null);
            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                ReadAll(_metadataReader.CustomAttributeCount, static (r, i) =>
                    r.GetCustomAttribute(CustomAttributeHandle.FromRowId(i))
                )
            );
            return field;
        }
        private set;
    }

    private ImmutableArray<T> ReadAll<T>(int count, Func<MetadataReader, int, T> reader)
    {
        Debug.Assert(_metadataReader is not null);
        var result = new T[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = reader(_metadataReader, i + 1);
        }
        return ImmutableCollectionsMarshal.AsImmutableArray(result);
    }

    public static AssemblyDefinition Load(IFileInfo filepath)
    {
        using var stream = filepath.CreateReadStream();
        var layout = ArchiveSerializer.Deserialize<MetadataLayout>(
            stream,
            ArchiveSerializerOptions.LittleEndian
        );
        return new AssemblyDefinition(in layout);
    }

    public async ValueTask WriteAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var layout = MetadataLayout.Create(this);
        await ArchiveSerializer.SerializeAsync(
            stream,
            layout,
            ArchiveSerializerOptions.LittleEndian,
            cancellationToken
        );
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
    [UseStaticMapper(typeof(NamespaceDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeDefinition.Mapper))]
    [UseStaticMapper(typeof(TypeMapper))]
    [UseStaticMapper(typeof(VariableDefinition.Mapper))]
    [UseStaticMapper(typeof(FunctionDefinition.Mapper))]
    [UseStaticMapper(typeof(ParameterDefinition.Mapper))]
    [UseStaticMapper(typeof(CustomAttributeDefinition.Mapper))]
    internal static partial class Mapper
    {
        public static partial AssemblyDefinition ToDefinition(AssemblyDefinitionBuilder builder);
    }
}
