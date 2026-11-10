using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Prism.Core.Diagnostics;
using Prism.Core.Metadata;
using Prism.Core.Syntax;
using Prism.Metadata;
using ZLinq;

namespace Prism.Core.Symbols.Metadata;

internal sealed class MetadataAssemblySymbol(AssemblyDefinition definition)
    : AssemblySymbol(definition.TargetInfo)
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

    public override ImmutableArray<Location> Locations
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(ref field, [new MetadataLocation(this)]);
            return field;
        }
    }

    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override AssemblyIdentity Identity => definition.Identity;

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
                GetOrAddNamespace(definition.GlobalNamespace),
                null
            );

            return field;
        }
    }

    internal MetadataNamespaceSymbol GetOrAddNamespace(NamespaceDefinition ns)
    {
        return _ownedNamespaces.GetOrAdd(
            ns,
            static (d, @this) => new MetadataNamespaceSymbol(d, @this),
            this
        );
    }

    internal MetadataNamedTypeSymbol GetOrAddType(TypeDefinition type)
    {
        return _ownedNamedTypes.GetOrAdd(
            type,
            static (d, @this) => new MetadataNamedTypeSymbol(d, @this),
            this
        );
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
        };
    }

    private NamedTypeSymbol GetReferencedType(NamedType descriptor)
    {
        return descriptor switch
        {
            TypeDefinition def => GetOrAddType(def),
            TypeReference reference => GetReferencedType(reference),
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
                    .UnifiedAssemblies.Single(u => u.OriginalReference == reference.Identity)
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

    internal MetadataVariableSymbol GetOrAddVariable(VariableDefinition variable)
    {
        return _ownedVariables.GetOrAdd(
            variable,
            static (d, @this) => new MetadataVariableSymbol(d, @this),
            this
        );
    }

    internal MetadataFunctionSymbol GetOrAddFunction(FunctionDefinition function)
    {
        return _ownedFunctions.GetOrAdd(
            function,
            static (d, @this) => new MetadataFunctionSymbol(d, @this),
            this
        );
    }

    internal Symbol GetOrAddFunctionOrAttribute(FunctionOrAttribute attribute)
    {
        return attribute switch
        {
            FunctionDefinition function => GetOrAddFunction(function),
            TypeDefinition type => GetOrAddType(type),
        };
    }

    internal MetadataParameterSymbol GetOrAddParameter(ParameterDefinition parameter)
    {
        return _ownedParameters.GetOrAdd(
            parameter,
            static (d, @this) => new MetadataParameterSymbol(d, @this),
            this
        );
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
            Debug.Assert(attributeType.Kind == NamedTypeKind.Attribute);
            using var parameters = attributeType
                .GetMembers()
                .AsValueEnumerable()
                .OfType<ParameterSymbol>()
                .ToArrayPool();

            Debug.Assert(parameters.Size == attribute.Arguments.Length);
            var arguments = parameters
                .Span.AsValueEnumerable()
                .Zip(attribute.Arguments.AsValueEnumerable())
                .Select(t => new AttributeArgument(t.First, t.Second))
                .ToImmutableArray();

            data[i] = new AttributeData(attributeType, arguments);
        }

        return ImmutableCollectionsMarshal.AsImmutableArray(data);
    }
}
