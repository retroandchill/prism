// @file VariableDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Constants;
using Prism.Metadata.BoundTree.Builders;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

public readonly union VariableDescriptorBuilder(VariableDefinitionBuilder, VariableReference)
{
    public VariableDescriptorHandle Handle
    {
        get
        {
            return this switch
            {
                VariableDefinitionBuilder def => def.Handle,
                VariableReference @ref => @ref.Handle,
            };
        }
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

    public MetadataExpressionBuilder? Initializer { get; set; }

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    public CustomAttributeDefinitionBuilder AddCustomAttribute(TypeDescriptorBuilder type)
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
