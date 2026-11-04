// @file MemberContainerReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal closed class MemberContainerReferenceBase(string name, MemberContainerReference parent)
{
    public string Name { get; } = name;

    public MemberContainerReference Parent { get; } = parent;
}

internal union NamespaceOrAssemblyReference(AssemblyReference, NamespaceReference)
{
    public static implicit operator MemberContainerReference(NamespaceOrAssemblyReference reference)
    {
        return reference switch
        {
            AssemblyReference assemblyReference => assemblyReference,
            NamespaceReference namespaceReference => namespaceReference,
        };
    }
}

internal union MemberContainerReference(AssemblyReference, NamespaceReference, TypeReference);

internal sealed class NamespaceReference(string name, NamespaceOrAssemblyReference parent)
    : MemberContainerReferenceBase(name, parent);

internal sealed class TypeReference(string name, MemberContainerReference parent)
    : MemberContainerReferenceBase(name, parent);
