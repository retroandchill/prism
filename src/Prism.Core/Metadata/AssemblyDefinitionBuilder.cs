// @file AssemblyDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal sealed class AssemblyDefinitionBuilder
{
    public AssemblyIdentity Identity { get; }

    public TargetInfo Target { get; }

    private readonly List<AssemblyReference> _references = [];
    private readonly List<NamespaceDefinitionBuilder> _namespaceDefs = [];
    private readonly List<NamespaceReference> _namespaceRefs = [];

    public AssemblyDefinitionBuilder(AssemblyIdentity identity, TargetInfo target)
    {
        Identity = identity;
        Target = target;
    }

    public AssemblyReference AddAssemblyReference(AssemblyIdentity identity)
    {
        var reference = new AssemblyReference(
            AssemblyRefHandle.FromRowId(_references.Count + 1),
            identity
        );
        _references.Add(reference);
        return reference;
    }

    public NamespaceDefinitionBuilder AddNamespacedDefinition(string name)
    {
        var ns = new NamespaceDefinitionBuilder(
            NamespaceDefHandle.FromRowId(_namespaceDefs.Count + 1),
            name
        );
        _namespaceDefs.Add(ns);
        return ns;
    }

    public NamespaceReference AddNamespaceReference(
        string name,
        NamespaceOrAssemblyReference parent
    )
    {
        var ns = new NamespaceReference(
            NamespaceRefHandle.FromRowId(_namespaceRefs.Count + 1),
            name,
            parent
        );
        _namespaceRefs.Add(ns);
        return ns;
    }
}
