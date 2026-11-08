// @file RowBuilders.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Core.Abi;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

public interface IAttributeHolderDefinitionBuilder
{
    ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes { get; }

    CustomAttributeDefinitionBuilder AddCustomAttribute(NamedTypeBuilder type);
}

public closed class MemberContainerDefinitionBuilder(
    string name,
    AssemblyDefinitionBuilder assembly
)
{
    private readonly List<TypeDefinitionBuilder> _types = [];
    private readonly List<VariableDefinitionBuilder> _variables = [];
    private readonly List<FunctionDefinitionBuilder> _functions = [];

    protected AssemblyDefinitionBuilder Assembly { get; } = assembly;

    public MemberContainerDefHandle Handle => GetHandle();

    public string Name { get; } = name;

    protected abstract MemberContainerDefHandle GetHandle();

    public ReadOnlyCollection<TypeDefinitionBuilder> Types => _types.AsReadOnly();
    public ReadOnlyCollection<VariableDefinitionBuilder> Variables => _variables.AsReadOnly();
    public ReadOnlyCollection<FunctionDefinitionBuilder> Functions => _functions.AsReadOnly();

    public TypeDefinitionBuilder AddType(string name)
    {
        var type = Assembly.AddTypeDefinition(name, this);
        _types.Add(type);
        return type;
    }

    public VariableDefinitionBuilder AddVariable(string name, TypeDescriptorBuilder type)
    {
        var variable = Assembly.AddVariable(name, type, this);
        _variables.Add(variable);
        return variable;
    }

    public FunctionDefinitionBuilder AddFunction(string name, TypeDescriptorBuilder returnType)
    {
        var function = Assembly.AddFunction(name, returnType, this);
        _functions.Add(function);
        return function;
    }
}

public sealed class NamespaceDefinitionBuilder(
    NamespaceDefHandle handle,
    string name,
    NamespaceDefinitionBuilder? parent,
    AssemblyDefinitionBuilder assembly
) : MemberContainerDefinitionBuilder(name, assembly)
{
    private readonly List<NamespaceDefinitionBuilder> _nestedNamespaces = [];
    public new NamespaceDefHandle Handle { get; } = handle;

    public NamespaceDefinitionBuilder? Parent { get; } = parent;

    public ReadOnlyCollection<NamespaceDefinitionBuilder> NestedNamespaces =>
        _nestedNamespaces.AsReadOnly();

    protected override MemberContainerDefHandle GetHandle()
    {
        return Handle;
    }

    public NamespaceDefinitionBuilder AddNestedNamespace(string name)
    {
        var definition = Assembly.AddNamespaceDefinition(name, this);
        _nestedNamespaces.Add(definition);
        return definition;
    }

    internal NamespaceDefinition ToDefinition(IReferenceHandler handler)
    {
        return NamespaceDefinition.Mapper.ToDefinition(this, handler);
    }
}

public sealed class TypeDefinitionBuilder(
    TypeDefHandle handle,
    string name,
    MemberContainerDefinitionBuilder parent,
    AssemblyDefinitionBuilder assembly
) : MemberContainerDefinitionBuilder(name, assembly), IAttributeHolderDefinitionBuilder
{
    private readonly List<ParameterDefinitionBuilder> _attributeParameters = [];
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];

    public new TypeDefHandle Handle { get; } = handle;

    public MemberContainerDefinitionBuilder Parent { get; } = parent;

    internal NamedTypeAttributes Attributes { get; private set; }

    public NamedTypeKind Kind
    {
        get => Attributes.Kind;
        set => Attributes = Attributes with { Kind = value };
    }

    public DeclaredVisibility Visibility
    {
        get => Attributes.Visibility;
        set => Attributes = Attributes with { Visibility = value };
    }

    public NamedTypeBuilder? BaseType { get; set; }

    public ReadOnlyCollection<ParameterDefinitionBuilder> AttributeParameters =>
        _attributeParameters.AsReadOnly();

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    protected override MemberContainerDefHandle GetHandle()
    {
        return Handle;
    }

    public ParameterDefinitionBuilder AddAttributeParameter(string name, TypeDescriptorBuilder type)
    {
        var parameter = Assembly.AddParameter(name, type, this);
        _attributeParameters.Add(parameter);
        return parameter;
    }

    public CustomAttributeDefinitionBuilder AddCustomAttribute(NamedTypeBuilder type)
    {
        var attribute = Assembly.AddCustomAttribute(type, this);
        _customAttributes.Add(attribute);
        return attribute;
    }

    internal TypeDefinition ToDefinition(IReferenceHandler handler)
    {
        return TypeDefinition.Mapper.ToDefinition(this, handler);
    }
}

public union NamedTypeBuilder(TypeDefinitionBuilder, TypeReference);

public union TypeDescriptorBuilder(TypeDefinitionBuilder, TypeReference, TypeSpecBuilder)
{
    public static implicit operator TypeDescriptorBuilder(NamedTypeBuilder builder)
    {
        return builder switch
        {
            TypeDefinitionBuilder b => b,
            TypeReference r => r,
        };
    }
}

public closed class TypeSpecBuilder;

public sealed class ArrayTypeBuilder : TypeSpecBuilder
{
    public TypeDescriptorBuilder ElementType { get; }

    public ulong? Size { get; }

    internal ArrayTypeBuilder(TypeDescriptorBuilder elementType, ulong? size)
    {
        ElementType = elementType;
        Size = size;
    }
}

public sealed class ReferenceTypeBuilder : TypeSpecBuilder
{
    public TypeDescriptorBuilder ElementType { get; }

    public bool IsMutable { get; }

    internal ReferenceTypeBuilder(TypeDescriptorBuilder elementType, bool isMutable)
    {
        ElementType = elementType;
        IsMutable = isMutable;
    }
}

public sealed class NullableTypeBuilder : TypeSpecBuilder
{
    public TypeDescriptorBuilder ElementType { get; }

    internal NullableTypeBuilder(TypeDescriptorBuilder elementType)
    {
        ElementType = elementType;
    }
}

public sealed class VariableDefinitionBuilder : IAttributeHolderDefinitionBuilder
{
    private readonly AssemblyDefinitionBuilder _assembly;
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];

    internal VariableDefinitionBuilder(
        VariableHandle handle,
        string name,
        TypeDescriptorBuilder type,
        MemberContainerDefinitionBuilder parent,
        AssemblyDefinitionBuilder assembly
    )
    {
        Handle = handle;
        Name = name;
        Type = type;
        Parent = parent;
        _assembly = assembly;
    }

    public VariableHandle Handle { get; }

    internal VariableAttributes Attributes { get; private set; }

    public VariableKind Kind
    {
        get => Attributes.Kind;
        set => Attributes = Attributes with { Kind = value };
    }

    public DeclaredVisibility Visibility
    {
        get => Attributes.Visibility;
        set => Attributes = Attributes with { Visibility = value };
    }

    public string Name { get; }

    public MemberContainerDefinitionBuilder Parent { get; }

    public TypeDescriptorBuilder Type { get; }

    public ConstantValue? ConstantValue { get; set; }

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    public CustomAttributeDefinitionBuilder AddCustomAttribute(NamedTypeBuilder type)
    {
        var attribute = _assembly.AddCustomAttribute(type, this);
        _customAttributes.Add(attribute);
        return attribute;
    }

    internal VariableDefinition ToDefinition(IReferenceHandler handler)
    {
        return VariableDefinition.Mapper.ToDefinition(this, handler);
    }
}

public sealed class FunctionDefinitionBuilder : IAttributeHolderDefinitionBuilder
{
    private readonly AssemblyDefinitionBuilder _assembly;
    private readonly List<ParameterDefinitionBuilder> _parameters = [];
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];

    internal FunctionDefinitionBuilder(
        FunctionHandle handle,
        string name,
        TypeDescriptorBuilder returnType,
        MemberContainerDefinitionBuilder parent,
        AssemblyDefinitionBuilder assembly
    )
    {
        Handle = handle;
        Name = name;
        ReturnType = returnType;
        Parent = parent;
        _assembly = assembly;
    }

    public FunctionHandle Handle { get; }

    internal FunctionAttributes Attributes { get; private set; }

    public DeclaredVisibility Visibility
    {
        get => Attributes.Visibility;
        set => Attributes = Attributes with { Visibility = value };
    }

    public bool IsExtern
    {
        get => Attributes.IsExtern;
        set => Attributes = Attributes with { IsExtern = value };
    }

    public AbiKind AbiKind
    {
        get => Attributes.AbiKind;
        set => Attributes = Attributes with { AbiKind = value };
    }

    public string Name { get; }

    public MemberContainerDefinitionBuilder Parent { get; }

    public TypeDescriptorBuilder ReturnType { get; }

    public TypeDescriptorBuilder? ReceiverType { get; set; }

    public ReadOnlyCollection<ParameterDefinitionBuilder> Parameters => _parameters.AsReadOnly();

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    public ParameterDefinitionBuilder AddParameter(string name, TypeDescriptorBuilder type)
    {
        var parameter = _assembly.AddParameter(name, type, this);
        _parameters.Add(parameter);
        return parameter;
    }

    public CustomAttributeDefinitionBuilder AddCustomAttribute(NamedTypeBuilder type)
    {
        var attribute = _assembly.AddCustomAttribute(type, this);
        _customAttributes.Add(attribute);
        return attribute;
    }

    internal FunctionDefinition ToDefinition(IReferenceHandler handler)
    {
        return FunctionDefinition.Mapper.ToDefinition(this, handler);
    }
}

public union FunctionOrAttributeBuilder(FunctionDefinitionBuilder, TypeDefinitionBuilder)
{
    public ParameterDefinitionBuilder AddParameter(string name, TypeDescriptorBuilder type)
    {
        return this switch
        {
            FunctionDefinitionBuilder function => function.AddParameter(name, type),
            TypeDefinitionBuilder typeDef => typeDef.AddAttributeParameter(name, type),
        };
    }
}

public sealed class ParameterDefinitionBuilder : IAttributeHolderDefinitionBuilder
{
    private readonly AssemblyDefinitionBuilder _assembly;
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];

    internal ParameterDefinitionBuilder(
        ParameterHandle handle,
        string name,
        TypeDescriptorBuilder type,
        FunctionOrAttributeBuilder parent,
        AssemblyDefinitionBuilder assembly
    )
    {
        Handle = handle;
        Name = name;
        Type = type;
        Parent = parent;
        _assembly = assembly;
    }

    public ParameterHandle Handle { get; }

    internal ParameterAttributes Attributes { get; private set; }

    public string Name { get; }

    public FunctionOrAttributeBuilder Parent { get; }

    public TypeDescriptorBuilder Type { get; }

    public ParameterDefaultDefinition? DefaultValue { get; set; }

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    public CustomAttributeDefinitionBuilder AddCustomAttribute(NamedTypeBuilder type)
    {
        var attribute = _assembly.AddCustomAttribute(type, this);
        _customAttributes.Add(attribute);
        return attribute;
    }

    internal ParameterDefinition ToDefinition(IReferenceHandler handler)
    {
        return ParameterDefinition.Mapper.ToDefinition(this, handler);
    }
}

public union AttributeHolderDefinitionBuilder(
    TypeDefinitionBuilder,
    VariableDefinitionBuilder,
    FunctionDefinitionBuilder,
    ParameterDefinitionBuilder
);

public sealed class CustomAttributeDefinitionBuilder
{
    internal CustomAttributeDefinitionBuilder(
        CustomAttributeHandle handle,
        NamedTypeBuilder type,
        AttributeHolderDefinitionBuilder parent
    )
    {
        Handle = handle;
        Type = type;
        Parent = parent;
    }

    public CustomAttributeHandle Handle { get; }

    public NamedTypeBuilder Type { get; }

    public AttributeHolderDefinitionBuilder Parent { get; }

    public List<ConstantValue> Arguments { get; } = [];
}
