// @file ParameterDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

public readonly union FunctionOrAttributeBuilder(FunctionDefinitionBuilder, TypeDefinitionBuilder)
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

    public CustomAttributeDefinitionBuilder AddCustomAttribute(TypeDescriptorBuilder type)
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
