// @file TypeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Prism.Core.Symbols;

namespace Prism.Core.Metadata;

internal union NamedType(TypeDefinition, TypeReference);

internal sealed class TypeDefinition(
    TypeDefHandle handle,
    NamedTypeAttributes attributes,
    string name,
    NamedTypeHandle baseType,
    MetadataReader reader
) : MemberContainerDefinition(name, reader)
{
    public NamedTypeKind Kind => attributes.Kind;

    public DeclaredVisibility Visibility => attributes.Visibility;

    private NamedType _lazyBaseType;
    public NamedType? BaseType
    {
        get
        {
            if (baseType.IsNone)
                return null;

            if (_lazyBaseType.Value is not null)
                return _lazyBaseType;

            Interlocked.CompareExchange(
                ref Unsafe.As<NamedType, object?>(ref _lazyBaseType),
                MetadataReader.GetNamedType(baseType).Value,
                null
            );
            return _lazyBaseType;
        }
    }

    protected override MemberContainerDefHandle Handle => handle;
}
