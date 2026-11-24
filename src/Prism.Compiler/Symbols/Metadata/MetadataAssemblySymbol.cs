using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Compiler.References;
using Prism.Metadata;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Utils;
using ZLinq;
using AssemblyDefinition = Prism.Metadata.AssemblyDefinition;
using FunctionDefinition = Prism.Metadata.FunctionDefinition;
using MemberContainerDefinition = Prism.Metadata.MemberContainerDefinition;
using NamespaceDefinition = Prism.Metadata.NamespaceDefinition;
using ParameterDefinition = Prism.Metadata.ParameterDefinition;
using TargetInfo = Prism.Configuration.TargetInfo;
using TypeDefinition = Prism.Metadata.TypeDefinition;
using VariableDefinition = Prism.Metadata.VariableDefinition;

namespace Prism.Compiler.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol : IAssemblySymbolData, ILocatableSymbolData
{
    private readonly ConcurrentDictionary<
        AssemblyReference,
        AssemblySymbol
    > _referencedAssemblySymbols = new();
    private readonly ConcurrentDictionary<
        NamespaceDefinition,
        MetadataNamespaceSymbol
    > _ownedNamespaces = new();
    private readonly ConcurrentDictionary<
        TypeDefinition,
        MetadataNamedTypeSymbol
    > _ownedNamedTypes = new();

    private readonly ConcurrentDictionary<TypeAlias, MetadataTypeAliasSymbol> _ownedTypeAliases =
        new();

    private readonly ConcurrentDictionary<
        NamespaceReference,
        NamespaceSymbol
    > _referencedNamespaces = new();
    private readonly ConcurrentDictionary<TypeReference, NamedTypeSymbol> _referencedTypes = new();

    private readonly ConcurrentDictionary<
        TypeSpecification,
        TypeSymbol
    > _referencedTypesSpecifications = new();

    private readonly ConcurrentDictionary<
        VariableDefinition,
        MetadataVariableSymbol
    > _ownedVariables = new();
    private readonly ConcurrentDictionary<
        FunctionDefinition,
        MetadataFunctionSymbol
    > _ownedFunctions = new();
    private readonly ConcurrentDictionary<
        ParameterDefinition,
        MetadataParameterSymbol
    > _ownedParameters = new();

    private readonly AssemblyDefinition _definition;
    private readonly ReferenceManager _referenceManager;

    public MetadataAssemblySymbol(AssemblyDefinition definition, ReferenceManager referenceManager)
    {
        _definition = definition;
        _referenceManager = referenceManager;
        PublicSymbol = new AssemblySymbol(this);
    }

    public AssemblySymbol PublicSymbol { get; }

    public string Name => _definition.Identity.Name;
    public Symbol? ContainingSymbol => null;

    public ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [new MetadataLocation(PublicSymbol)]
            );
            return field;
        }
    }

    public AssemblyIdentity Identity => _definition.Identity;
    public TargetInfo TargetInfo => _definition.TargetInfo;

    public AssemblyReferences References
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, this, static @this =>
                @this._referenceManager.ResolveAssemblyReference(
                    @this._definition.ReferencedAssemblies
                )
            );
        }
    }

    public NamespaceSymbol GlobalNamespace
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            Interlocked.CompareExchange(
                ref field,
                GetOrAddNamespace(_definition.GlobalNamespace),
                null
            );

            return field;
        }
    }

    internal NamespaceSymbol GetOrAddNamespace(NamespaceDefinition ns)
    {
        return _ownedNamespaces
            .GetOrAdd(ns, static (d, @this) => new MetadataNamespaceSymbol(d, @this), this)
            .PublicSymbol;
    }

    internal NamedTypeSymbol GetOrAddType(TypeDefinition type)
    {
        return _ownedNamedTypes
            .GetOrAdd(type, static (d, @this) => new MetadataNamedTypeSymbol(d, @this), this)
            .PublicSymbol;
    }

    internal TypeAliasSymbol GetOrAddTypeAlias(TypeAlias typeAlias)
    {
        return _ownedTypeAliases
            .GetOrAdd(typeAlias, static (d, @this) => new MetadataTypeAliasSymbol(d, @this), this)
            .PublicSymbol;
    }

    internal MemberContainerSymbol GetOrAddMemberContainer(MemberContainerDefinition container)
    {
        return container switch
        {
            NamespaceDefinition ns => GetOrAddNamespace(ns),
            TypeDefinition type => GetOrAddType(type),
        };
    }

    internal TypeSymbol GetReferencedType(TypeDescriptor descriptor)
    {
        return descriptor switch
        {
            TypeDefinition def => GetOrAddType(def),
            TypeReference reference => GetReferencedType(reference),
            TypeSpecification specification => GetReferencedTypeSpecification(specification),
            TypeAlias alias => GetOrAddTypeAlias(alias),
        };
    }

    private NamedTypeSymbol GetReferencedType(TypeReference descriptor)
    {
        return _referencedTypes.GetOrAdd(
            descriptor,
            static (d, @this) =>
            {
                var container = @this.GetContainer(d.Parent);
                return container
                    .GetMembers(d.Name)
                    .AsValueEnumerable()
                    .OfType<NamedTypeSymbol>()
                    .Single();
            },
            this
        );
    }

    private MemberContainerSymbol GetContainer(MemberContainerReference reference)
    {
        return reference switch
        {
            AssemblyReference asm => GetReferencedAssembly(asm).GlobalNamespace,
            NamespaceReference ns => GetReferencedNamespace(ns),
            TypeReference type => GetReferencedType(type),
        };
    }

    private NamespaceSymbol GetReferencedNamespace(NamespaceReference reference)
    {
        return _referencedNamespaces.GetOrAdd(
            reference,
            static (d, @this) =>
            {
                var container = @this.GetContainer(d.Parent);
                return container
                    .GetMembers(d.Name)
                    .AsValueEnumerable()
                    .OfType<NamespaceSymbol>()
                    .Single();
            },
            this
        );
    }

    private AssemblySymbol GetReferencedAssembly(AssemblyReference reference)
    {
        return _referencedAssemblySymbols.GetOrAdd(
            reference,
            static (_, ctx) =>
            {
                var (@this, reference) = ctx;
                return @this
                    .References.UnifiedAssemblies.Single(u =>
                        u.OriginalReference == reference.Identity
                    )
                    .TargetAssembly;
            },
            (this, reference)
        );
    }

    private TypeSymbol GetReferencedTypeSpecification(TypeSpecification specification)
    {
        return _referencedTypesSpecifications.GetOrAdd(
            specification,
            static (d, @this) =>
            {
                return d switch
                {
                    ArrayTypeSpecification array => new ArrayTypeSymbol(
                        @this.GetReferencedType(array.ElementType),
                        array.Size
                    ),
                    ReferenceTypeSpecification reference => new ReferenceTypeSymbol(
                        @this.GetReferencedType(reference.ElementType),
                        reference.IsMutable
                    ),
                    NullableTypeSpecification nullable => new NullableTypeSymbol(
                        @this.GetReferencedType(nullable.ElementType)
                    ),
                };
            },
            this
        );
    }

    internal VariableSymbol GetOrAddVariable(VariableDefinition variable)
    {
        return _ownedVariables
            .GetOrAdd(variable, static (d, @this) => new MetadataVariableSymbol(d, @this), this)
            .PublicSymbol;
    }

    internal FunctionSymbol GetOrAddFunction(FunctionDefinition function)
    {
        return _ownedFunctions
            .GetOrAdd(function, static (d, @this) => new MetadataFunctionSymbol(d, @this), this)
            .PublicSymbol;
    }

    internal Symbol GetOrAddFunctionOrAttribute(FunctionOrAttribute attribute)
    {
        return attribute switch
        {
            FunctionDefinition function => GetOrAddFunction(function),
            TypeDefinition type => GetOrAddType(type),
        };
    }

    internal ParameterSymbol GetOrAddParameter(ParameterDefinition parameter)
    {
        return _ownedParameters
            .GetOrAdd(parameter, static (d, @this) => new MetadataParameterSymbol(d, @this), this)
            .PublicSymbol;
    }

    internal ImmutableArray<AttributeData> GetAttributes(IAttributeHolderDefinition holder)
    {
        var attributes = holder.CustomAttributes;

        if (attributes.IsEmpty)
            return [];

        var data = new AttributeData[attributes.Length];
        foreach (var (i, attribute) in attributes.AsValueEnumerable().Index())
        {
            var attributeType = GetReferencedType(attribute.Type);
            var unwrapped = (NamedTypeSymbol)attributeType.UnwrapTypeAlias();
            Debug.Assert(unwrapped.Kind == NamedTypeKind.Attribute);
            var parameters = unwrapped.AttributeParameters;

            Debug.Assert(parameters.Length == attribute.Arguments.Length);
            var arguments = parameters
                .AsValueEnumerable()
                .Zip(attribute.Arguments.AsValueEnumerable())
                .Select(t => new AttributeArgument(t.First, t.Second))
                .ToImmutableArray();

            data[i] = new AttributeData(attributeType, arguments);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(data);
    }
}
