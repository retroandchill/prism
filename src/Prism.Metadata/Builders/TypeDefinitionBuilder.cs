// @file TypeDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

public readonly union TypeDescriptorBuilder(
    TypeDefinitionBuilder,
    TypeReference,
    TypeSpecBuilder,
    TypeAliasBuilder
);

public sealed class TypeDefinitionBuilder(
    TypeDefHandle handle,
    string name,
    MemberContainerDefinitionBuilder parent,
    AssemblyDefinitionBuilder assembly
) : MemberContainerDefinitionBuilder(name, assembly), IAttributeHolderDefinitionBuilder
{
    private readonly List<EnumOrdinalDefinitionBuilder> _enumOrdinals = [];
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

    public SpecialType SpecialType
    {
        get => Attributes.SpecialType;
        set => Attributes = Attributes with { SpecialType = value };
    }

    public TypeDescriptorBuilder? BaseType { get; set; }

    public ReadOnlyCollection<EnumOrdinalDefinitionBuilder> EnumOrdinals =>
        _enumOrdinals.AsReadOnly();

    public ReadOnlyCollection<ParameterDefinitionBuilder> AttributeParameters =>
        _attributeParameters.AsReadOnly();

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    protected override MemberContainerDefHandle GetHandle()
    {
        return Handle;
    }

    public EnumOrdinalDefinitionBuilder AddEnumOrdinal(string name, ulong bits)
    {
        var ordinal = Assembly.AddEnumOrdinal(name, bits, this);
        _enumOrdinals.Add(ordinal);
        return ordinal;
    }

    public ParameterDefinitionBuilder AddAttributeParameter(string name, TypeDescriptorBuilder type)
    {
        var parameter = Assembly.AddParameter(name, type, this);
        _attributeParameters.Add(parameter);
        return parameter;
    }

    public CustomAttributeDefinitionBuilder AddCustomAttribute(TypeDescriptorBuilder type)
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
