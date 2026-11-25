// @file NamespaceDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Metadata.Symbols;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Metadata.Builders;

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
