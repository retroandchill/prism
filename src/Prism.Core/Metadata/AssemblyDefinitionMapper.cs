// @file AssemblyDefinitionMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, UseReferenceHandling = true)]
internal static partial class AssemblyDefinitionMapper
{
    public static partial AssemblyDefinition ToDefinition(this AssemblyDefinitionBuilder builder);

    [MapDerivedType<NamespaceDefinitionBuilder, NamespaceDefinition>]
    [MapDerivedType<TypeDefinitionBuilder, TypeDefinition>]
    private static partial MemberContainerDefinition ToDefinition(
        this MemberContainerDefinitionBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    [UserMapping(Default = true)]
    private static NamespaceDefinition ToDefinition(
        this NamespaceDefinitionBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    )
    {
        // Mapperly has a bug where it doesn't like recursive types even with reference handling enabled, so we're using
        // a separate mapper that maps the non-recursive properties and then we manually handle those two.
        var mapped = MapNamespaceWithoutRecursion(builder, handler);
        mapped.Parent = builder.Parent?.ToDefinition(handler);
        mapped.NestedNamespaces =
        [
            .. builder.NestedNamespaces.Select(n => n.ToDefinition(handler)),
        ];
        return mapped;
    }

    [MapperIgnoreSource(nameof(NamespaceDefinitionBuilder.Parent))]
    [MapperIgnoreSource(nameof(NamespaceDefinitionBuilder.NestedNamespaces))]
    [MapperIgnoreTarget(nameof(NamespaceDefinition.Parent))]
    [MapperIgnoreTarget(nameof(NamespaceDefinition.NestedNamespaces))]
    private static partial NamespaceDefinition MapNamespaceWithoutRecursion(
        NamespaceDefinitionBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    private static partial TypeDefinition ToDefinition(
        this TypeDefinitionBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    [MapDerivedType<ArrayTypeBuilder, ArrayTypeSpecification>]
    [MapDerivedType<ReferenceTypeBuilder, ReferenceTypeSpecification>]
    [MapDerivedType<NullableTypeBuilder, NullableTypeSpecification>]
    private static partial TypeSpecification ToDefinition(
        TypeSpecBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    private static partial ArrayTypeSpecification ToDefinition(
        this ArrayTypeBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    private static partial ReferenceTypeSpecification ToDefinition(
        this ReferenceTypeBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    private static partial NullableTypeSpecification ToDefinition(
        this NullableTypeBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    );

    private static NamedType ToDefinition(
        this NamedTypeBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    )
    {
        return builder switch
        {
            TypeDefinitionBuilder b => b.ToDefinition(handler),
            TypeReference r => r,
        };
    }

    private static TypeDescriptor ToDefinition(
        this TypeDescriptorBuilder builder,
        [ReferenceHandler] IReferenceHandler handler
    )
    {
        return builder switch
        {
            TypeDefinitionBuilder b => b.ToDefinition(handler),
            TypeReference r => r,
            NullableTypeBuilder b => b.ToDefinition(handler),
            ReferenceTypeBuilder b => b.ToDefinition(handler),
            ArrayTypeBuilder b => b.ToDefinition(handler),
        };
    }
}
