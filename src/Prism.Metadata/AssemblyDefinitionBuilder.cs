// @file AssemblyDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;

namespace Prism.Metadata;

public sealed class AssemblyDefinitionBuilder
{
    public Prism.AssemblyIdentity Identity { get; }

    public TargetInfo TargetInfo { get; }

    private readonly List<AssemblyReference> _references = [];
    private readonly List<NamespaceDefinitionBuilder> _namespaceDefs = [];
    private readonly List<NamespaceReference> _namespaceRefs = [];

    private readonly List<TypeDefinitionBuilder> _typeDefs = [];
    private readonly List<TypeReference> _typeRefs = [];
    private readonly List<TypeSpecBuilder> _typeSpecs = [];

    private readonly List<VariableDefinitionBuilder> _variables = [];
    private readonly List<FunctionDefinitionBuilder> _functions = [];
    private readonly List<ParameterDefinitionBuilder> _parameters = [];
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];

    public AssemblyDefinitionBuilder(Prism.AssemblyIdentity identity, TargetInfo targetInfo)
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

    public AssemblyReference AddAssemblyReference(Prism.AssemblyIdentity identity)
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

    internal ReadOnlyCollection<VariableDefinitionBuilder> Variables => _variables.AsReadOnly();

    internal ReadOnlyCollection<FunctionDefinitionBuilder> Functions => _functions.AsReadOnly();

    internal ReadOnlyCollection<ParameterDefinitionBuilder> Parameters => _parameters.AsReadOnly();

    internal ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

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

    internal CustomAttributeDefinitionBuilder AddCustomAttribute(
        NamedTypeBuilder type,
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

    public AssemblyDefinition Build()
    {
        return AssemblyDefinition.Mapper.ToDefinition(this);
    }
}
