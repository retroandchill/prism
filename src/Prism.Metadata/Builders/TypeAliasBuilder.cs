// @file TypeAliasBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

public sealed class TypeAliasBuilder(
    TypeAliasHandle handle,
    string name,
    MemberContainerDefinitionBuilder container
)
{
    public TypeAliasHandle Handle { get; } = handle;

    public string Name { get; } = name;

    internal TypeAliasAttributes Attributes { get; set; }

    public DeclaredVisibility Visibility
    {
        get => Attributes.Visibility;
        set => Attributes = Attributes with { Visibility = value };
    }

    public MemberContainerDefinitionBuilder Container { get; } = container;

    public TypeDescriptorBuilder AliasedType { get; set; }

    public TypeAlias ToDefinition(IReferenceHandler handler)
    {
        return TypeAlias.Mapper.ToDefinition(this, handler);
    }
}
