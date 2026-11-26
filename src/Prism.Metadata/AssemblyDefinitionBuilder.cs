// @file AssemblyDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Prism.Configuration;
using Prism.Metadata.BoundTree.Builders;
using Prism.Metadata.Builders;
using Prism.Metadata.Symbols;

namespace Prism.Metadata;

public sealed class AssemblyDefinitionBuilder
{
    public AssemblyIdentity Identity { get; }

    public TargetInfo TargetInfo { get; }

    private readonly List<AssemblyReference> _references = [];
    private readonly List<NamespaceDefinitionBuilder> _namespaceDefs = [];
    private readonly List<NamespaceReference> _namespaceRefs = [];

    private readonly List<TypeDefinitionBuilder> _typeDefs = [];
    private readonly List<TypeReference> _typeRefs = [];
    private readonly List<TypeSpecBuilder> _typeSpecs = [];
    private readonly List<TypeAliasBuilder> _typeAliases = [];

    private readonly List<VariableDefinitionBuilder> _variables = [];
    private readonly List<FunctionDefinitionBuilder> _functions = [];
    private readonly List<ParameterDefinitionBuilder> _parameters = [];
    private readonly List<EnumOrdinalDefinitionBuilder> _enumOrdinals = [];
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];

    private readonly List<VariableReference> _variableRefs = [];
    private readonly List<FunctionReferenceBuilder> _functionRefs = [];

    private readonly Dictionary<VariableHandle, MetadataExpressionBuilder> _variableInitializers =
    [];

    public AssemblyDefinitionBuilder(AssemblyIdentity identity, TargetInfo targetInfo)
    {
        Identity = identity;
        TargetInfo = targetInfo;
        GlobalNamespace = new NamespaceDefinitionBuilder(
            NamespaceDefHandle.None,
            CommonNames.Global,
            null,
            this
        );
    }

    public ReadOnlyCollection<AssemblyReference> ReferencedAssemblies => _references.AsReadOnly();

    public AssemblyReference AddAssemblyReference(AssemblyIdentity identity)
    {
        var reference = new AssemblyReference(
            AssemblyRefHandle.FromRowId(_references.Count + 1),
            identity
        );
        _references.Add(reference);
        return reference;
    }

    public NamespaceDefinitionBuilder GlobalNamespace { get; }

    internal ReadOnlyCollection<NamespaceDefinitionBuilder> DefinedNamespaces =>
        _namespaceDefs.AsReadOnly();

    internal ReadOnlyCollection<NamespaceReference> ReferencedNamespaces =>
        _namespaceRefs.AsReadOnly();

    internal ReadOnlyCollection<TypeDefinitionBuilder> DefinedTypes => _typeDefs.AsReadOnly();

    internal ReadOnlyCollection<TypeReference> ReferencedTypes => _typeRefs.AsReadOnly();

    internal ReadOnlyCollection<TypeSpecBuilder> TypeSpecifications => _typeSpecs.AsReadOnly();

    internal ReadOnlyCollection<TypeAliasBuilder> TypeAliases => _typeAliases.AsReadOnly();

    internal ReadOnlyCollection<VariableDefinitionBuilder> Variables => _variables.AsReadOnly();

    internal ReadOnlyCollection<FunctionDefinitionBuilder> Functions => _functions.AsReadOnly();

    internal ReadOnlyCollection<ParameterDefinitionBuilder> Parameters => _parameters.AsReadOnly();

    internal ReadOnlyCollection<EnumOrdinalDefinitionBuilder> EnumOrdinals =>
        _enumOrdinals.AsReadOnly();

    internal ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    internal ReadOnlyCollection<VariableReference> ReferencedVariables =>
        _variableRefs.AsReadOnly();

    internal ReadOnlyCollection<FunctionReferenceBuilder> ReferencedFunctions =>
        _functionRefs.AsReadOnly();

    internal ReadOnlyDictionary<VariableHandle, MetadataExpressionBuilder> VariableInitializers =>
        _variableInitializers.AsReadOnly();

    internal NamespaceDefinitionBuilder AddNamespaceDefinition(
        string name,
        NamespaceDefinitionBuilder parent
    )
    {
        var ns = new NamespaceDefinitionBuilder(
            NamespaceDefHandle.FromRowId(_namespaceDefs.Count + 1),
            name,
            parent,
            this
        );
        _namespaceDefs.Add(ns);
        return ns;
    }

    public NamespaceReference AddNamespaceReference(
        string name,
        NamespaceOrAssemblyReference parent
    )
    {
        var ns = new NamespaceReference(
            NamespaceRefHandle.FromRowId(_namespaceRefs.Count + 1),
            name,
            parent
        );
        _namespaceRefs.Add(ns);
        return ns;
    }

    internal TypeDefinitionBuilder AddTypeDefinition(
        string name,
        MemberContainerDefinitionBuilder parent
    )
    {
        var type = new TypeDefinitionBuilder(
            TypeDefHandle.FromRowId(_typeDefs.Count + 1),
            name,
            parent,
            this
        );
        _typeDefs.Add(type);
        return type;
    }

    public TypeReference AddTypeReference(string name, MemberContainerReference parent)
    {
        var type = new TypeReference(TypeRefHandle.FromRowId(_typeRefs.Count + 1), name, parent);
        _typeRefs.Add(type);
        return type;
    }

    public ArrayTypeBuilder AddArrayType(TypeDescriptorBuilder elementType, ulong? length)
    {
        var type = new ArrayTypeBuilder(elementType, length);
        _typeSpecs.Add(type);
        return type;
    }

    public ReferenceTypeBuilder AddReferenceType(TypeDescriptorBuilder elementType, bool isMutable)
    {
        var type = new ReferenceTypeBuilder(elementType, isMutable);
        _typeSpecs.Add(type);
        return type;
    }

    public NullableTypeBuilder AddNullableType(TypeDescriptorBuilder elementType)
    {
        var type = new NullableTypeBuilder(elementType);
        _typeSpecs.Add(type);
        return type;
    }

    public TypeAliasBuilder AddTypeAlias(string name, MemberContainerDefinitionBuilder parent)
    {
        var typeAlias = new TypeAliasBuilder(
            TypeAliasHandle.FromRowId(_typeAliases.Count + 1),
            name,
            parent
        );
        _typeAliases.Add(typeAlias);
        return typeAlias;
    }

    internal VariableDefinitionBuilder AddVariable(
        string name,
        TypeDescriptorBuilder type,
        MemberContainerDefinitionBuilder parent
    )
    {
        var variable = new VariableDefinitionBuilder(
            VariableHandle.FromRowId(_variables.Count + 1),
            name,
            type,
            parent,
            this
        );
        _variables.Add(variable);
        return variable;
    }

    internal FunctionDefinitionBuilder AddFunction(
        string name,
        TypeDescriptorBuilder returnType,
        MemberContainerDefinitionBuilder parent
    )
    {
        var function = new FunctionDefinitionBuilder(
            FunctionHandle.FromRowId(_functions.Count + 1),
            name,
            returnType,
            parent,
            this
        );
        _functions.Add(function);
        return function;
    }

    internal ParameterDefinitionBuilder AddParameter(
        string name,
        TypeDescriptorBuilder type,
        FunctionOrAttributeBuilder parent
    )
    {
        var param = new ParameterDefinitionBuilder(
            ParameterHandle.FromRowId(_parameters.Count + 1),
            name,
            type,
            parent,
            this
        );
        _parameters.Add(param);
        return param;
    }

    internal EnumOrdinalDefinitionBuilder AddEnumOrdinal(
        string name,
        ulong bits,
        TypeDefinitionBuilder type
    )
    {
        var ordinal = new EnumOrdinalDefinitionBuilder(
            EnumOrdinalHandle.FromRowId(_enumOrdinals.Count + 1),
            name,
            bits,
            type,
            this
        );
        _enumOrdinals.Add(ordinal);
        return ordinal;
    }

    internal CustomAttributeDefinitionBuilder AddCustomAttribute(
        TypeDescriptorBuilder type,
        AttributeHolderDefinitionBuilder parent
    )
    {
        var attribute = new CustomAttributeDefinitionBuilder(
            CustomAttributeHandle.FromRowId(_customAttributes.Count + 1),
            type,
            parent
        );
        _customAttributes.Add(attribute);
        return attribute;
    }

    public VariableReference AddVariableReference(string name, MemberContainerReference container)
    {
        var variable = new VariableReference(
            VariableRefHandle.FromRowId(_variableRefs.Count + 1),
            name,
            container
        );
        _variableRefs.Add(variable);
        return variable;
    }

    public FunctionReferenceBuilder AddFunctionReference(
        string name,
        MemberContainerReference container
    )
    {
        var function = new FunctionReferenceBuilder(
            FunctionRefHandle.FromRowId(_functionRefs.Count + 1),
            name,
            container
        );
        _functionRefs.Add(function);
        return function;
    }

    internal void SetVariableInitializer(
        VariableHandle handle,
        MetadataExpressionBuilder? initializer
    )
    {
        if (initializer is not null)
        {
            _variableInitializers[handle] = initializer;
        }
        else
        {
            _variableInitializers.Remove(handle);
        }
    }

    public AssemblyDefinition Build()
    {
        return AssemblyDefinition.Mapper.ToDefinition(this);
    }
}
