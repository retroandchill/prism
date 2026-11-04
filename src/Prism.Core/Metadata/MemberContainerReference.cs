// @file MemberContainerReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace Prism.Core.Metadata;

internal closed class MemberContainerReferenceBase(
    string name,
    MemberContainerRefHandle parent,
    MetadataReader reader
)
{
    public string Name { get; } = name;

    public MemberContainerReference Parent
    {
        get
        {
            if (field.Value is not null)
                return field;

            var reference = reader.GetMemberContainerRef(parent);
            Interlocked.CompareExchange(
                ref Unsafe.As<MemberContainerReference, object?>(ref field),
                reference.Value,
                null
            );
            return field;
        }
    }
}

internal union MemberContainerReference(AssemblyReference, NamespaceReference, TypeReference);

internal sealed class NamespaceReference(
    string name,
    NamespaceOrAssemblyRefHandle parent,
    MetadataReader reader
) : MemberContainerReferenceBase(name, parent, reader);

internal sealed class TypeReference(
    string name,
    MemberContainerRefHandle parent,
    MetadataReader reader
) : MemberContainerReferenceBase(name, parent, reader);
