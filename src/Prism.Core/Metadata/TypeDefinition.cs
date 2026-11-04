// @file TypeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal union NamedType(TypeDefinition, TypeReference);

internal sealed class TypeDefinition(TypeDefHandle handle, string name, AssemblyDefinition assembly)
    : MemberContainerDefinition(name, assembly)
{
    private Lazy<NamedType?>? _baseType;
    public NamedType? BaseType
    {
        get
        {
            if (_baseType is not null)
                return _baseType.Value;

            Interlocked.CompareExchange(
                ref _baseType,
                new Lazy<NamedType?>(
                    () => Assembly.GetBaseType(handle),
                    LazyThreadSafetyMode.PublicationOnly
                ),
                null
            );
            return _baseType.Value;
        }
    }

    protected override MemberContainerDefHandle Handle => handle;
}
