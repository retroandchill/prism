// @file TypeSpecification.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal union TypeDescriptor(TypeDefinition, TypeReference, TypeSpecification);

internal closed record TypeSpecification;

internal sealed record ArrayTypeSpecification(TypeDescriptor ElementType, ulong? Size)
    : TypeSpecification;

internal sealed record ReferenceTypeSpecification(TypeDescriptor ElementType, bool IsMutable)
    : TypeSpecification;

internal sealed record NullableTypeSpecification(TypeDescriptor ElementType) : TypeSpecification;
