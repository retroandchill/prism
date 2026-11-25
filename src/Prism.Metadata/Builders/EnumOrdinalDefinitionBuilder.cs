// @file RowBuilders.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using EnumOrdinalDefinition = Prism.Metadata.Symbols.EnumOrdinalDefinition;

namespace Prism.Metadata.Builders;

public sealed class EnumOrdinalDefinitionBuilder : IAttributeHolderDefinitionBuilder
{
    private readonly List<CustomAttributeDefinitionBuilder> _customAttributes = [];
    private readonly AssemblyDefinitionBuilder _assembly;

    internal EnumOrdinalDefinitionBuilder(
        EnumOrdinalHandle handle,
        string name,
        ulong bits,
        TypeDefinitionBuilder parent,
        AssemblyDefinitionBuilder assembly
    )
    {
        Handle = handle;
        Name = name;
        Bits = bits;
        Parent = parent;
        _assembly = assembly;
    }

    public EnumOrdinalHandle Handle { get; }

    public string Name { get; }

    public TypeDefinitionBuilder Parent { get; }

    public ulong Bits { get; }

    public ReadOnlyCollection<CustomAttributeDefinitionBuilder> CustomAttributes =>
        _customAttributes.AsReadOnly();

    public CustomAttributeDefinitionBuilder AddCustomAttribute(TypeDescriptorBuilder type)
    {
        var attribute = _assembly.AddCustomAttribute(type, this);
        _customAttributes.Add(attribute);
        return attribute;
    }

    internal EnumOrdinalDefinition ToDefinition(IReferenceHandler handler)
    {
        return EnumOrdinalDefinition.Mapper.ToDefinition(this, handler);
    }
}
