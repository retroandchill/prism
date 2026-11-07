// @file RowBuilders.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

internal closed class MemberContainerDefinitionBuilder(string name)
{
    public MemberContainerDefHandle Handle => GetHandle();

    public string Name { get; set; } = name;

    protected abstract MemberContainerDefHandle GetHandle();
}

internal sealed class NamespaceDefinitionBuilder(NamespaceDefHandle handle, string name)
    : MemberContainerDefinitionBuilder(name)
{
    public new NamespaceDefHandle Handle { get; } = handle;

    public NamespaceDefinitionBuilder? Parent { get; set; }

    protected override MemberContainerDefHandle GetHandle()
    {
        return Handle;
    }
}

internal sealed class TypeDefinitionBuilder(TypeDefHandle handle, string name)
    : MemberContainerDefinitionBuilder(name)
{
    public new TypeDefHandle Handle { get; } = handle;

    public MemberContainerDefinition? Parent { get; set; }

    public NamedTypeAttributes Attributes { get; private set; }

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

    protected override MemberContainerDefHandle GetHandle()
    {
        return Handle;
    }
}
