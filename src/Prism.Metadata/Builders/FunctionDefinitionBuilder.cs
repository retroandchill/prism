// @file FunctionDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

public readonly union FunctionDescriptorBuilder(FunctionDefinitionBuilder, FunctionReferenceBuilder)
{
    public FunctionDescriptorHandle Handle
    {
        get
        {
            return this switch
            {
                FunctionDefinitionBuilder def => def.Handle,
                FunctionReferenceBuilder @ref => @ref.Handle,
            };
        }
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

    public FunctionKind Kind
    {
        get => Attributes.Kind;
        set => Attributes = Attributes with { Kind = value };
    }

    public bool IsDefaultDefined
    {
        get => Attributes.IsDefaultDefined;
        set => Attributes = Attributes with { IsDefaultDefined = value };
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

    public CustomAttributeDefinitionBuilder AddCustomAttribute(TypeDescriptorBuilder type)
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
