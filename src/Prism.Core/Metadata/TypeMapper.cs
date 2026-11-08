// @file TypeMapper.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

[Mapper(UseReferenceHandling = true)]
internal static partial class TypeMapper
{
    [MapDerivedType<ArrayTypeBuilder, ArrayTypeSpecification>]
    [MapDerivedType<ReferenceTypeBuilder, ReferenceTypeSpecification>]
    [MapDerivedType<NullableTypeBuilder, NullableTypeSpecification>]
    public static partial TypeSpecification ToDefinition(
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

    public static NamedType ToDefinition(
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

    public static TypeDescriptor ToDefinition(
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
