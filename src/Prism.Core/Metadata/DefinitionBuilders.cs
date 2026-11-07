// @file RowBuilders.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

public closed class MemberContainerDefinitionBuilder(
    string name,
    AssemblyDefinitionBuilder assembly
)
{
    private readonly List<TypeDefinitionBuilder> _types = [];
    protected AssemblyDefinitionBuilder Assembly { get; } = assembly;

    public MemberContainerDefHandle Handle => GetHandle();

    public string Name { get; } = name;

    protected abstract MemberContainerDefHandle GetHandle();

    public ReadOnlyCollection<TypeDefinitionBuilder> Types => _types.AsReadOnly();

    public TypeDefinitionBuilder AddType(string name)
    {
        var type = Assembly.AddTypeDefinition(name, this);
        _types.Add(type);
        return type;
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
}

public sealed class TypeDefinitionBuilder(
    TypeDefHandle handle,
    string name,
    MemberContainerDefinitionBuilder parent,
    AssemblyDefinitionBuilder assembly
) : MemberContainerDefinitionBuilder(name, assembly)
{
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

    protected override MemberContainerDefHandle GetHandle()
    {
        return Handle;
    }
}

public union NamedTypeBuilder(TypeDefinitionBuilder, TypeReference);

public union TypeDescriptorBuilder(TypeDefinitionBuilder, TypeReference, TypeSpecBuilder);

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
