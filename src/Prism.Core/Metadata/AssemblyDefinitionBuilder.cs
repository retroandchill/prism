// @file AssemblyDefinitionBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using Prism.Core.Utils;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Prism.Core.Metadata;

public sealed class AssemblyDefinitionBuilder
{
    public AssemblyIdentity Identity { get; }

    public TargetInfo TargetInfo { get; }

    private readonly List<AssemblyReference> _references = [];
    private readonly List<NamespaceDefinitionBuilder> _namespaceDefs = [];
    private readonly List<NamespaceReference> _namespaceRefs = [];

    private readonly List<TypeDefinitionBuilder> _typeDefs = [];
    private readonly List<TypeReference> _typeRefs = [];
    private readonly List<TypeSpecBuilder> _typeSpecs = [];

    public AssemblyDefinitionBuilder(AssemblyIdentity identity, TargetInfo targetInfo)
    {
        Identity = identity;
        TargetInfo = targetInfo;
        GlobalNamespace = new NamespaceDefinitionBuilder(
            NamespaceDefHandle.None,
            CommonNames.Global,
            null,
            this
        );
    }

    public ReadOnlyCollection<AssemblyReference> ReferencedAssemblies => _references.AsReadOnly();

    public AssemblyReference AddAssemblyReference(AssemblyIdentity identity)
    {
        var reference = new AssemblyReference(
            AssemblyRefHandle.FromRowId(_references.Count + 1),
            identity
        );
        _references.Add(reference);
        return reference;
    }

    public NamespaceDefinitionBuilder GlobalNamespace { get; }

    internal ReadOnlyCollection<NamespaceDefinitionBuilder> DefinedNamespaces =>
        _namespaceDefs.AsReadOnly();

    internal ReadOnlyCollection<NamespaceReference> ReferencedNamespaces =>
        _namespaceRefs.AsReadOnly();

    internal ReadOnlyCollection<TypeDefinitionBuilder> DefinedTypes => _typeDefs.AsReadOnly();

    internal ReadOnlyCollection<TypeReference> ReferencedTypes => _typeRefs.AsReadOnly();

    internal ReadOnlyCollection<TypeSpecBuilder> TypeSpecifications => _typeSpecs.AsReadOnly();

    internal NamespaceDefinitionBuilder AddNamespaceDefinition(
        string name,
        NamespaceDefinitionBuilder parent
    )
    {
        var ns = new NamespaceDefinitionBuilder(
            NamespaceDefHandle.FromRowId(_namespaceDefs.Count + 1),
            name,
            parent,
            this
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

    internal TypeDefinitionBuilder AddTypeDefinition(
        string name,
        MemberContainerDefinitionBuilder parent
    )
    {
        var type = new TypeDefinitionBuilder(
            TypeDefHandle.FromRowId(_typeDefs.Count + 1),
            name,
            parent,
            this
        );
        _typeDefs.Add(type);
        return type;
    }

    public TypeReference AddTypeReference(string name, MemberContainerReference parent)
    {
        var type = new TypeReference(TypeRefHandle.FromRowId(_typeRefs.Count + 1), name, parent);
        _typeRefs.Add(type);
        return type;
    }

    public AssemblyDefinition Build()
    {
        return this.ToDefinition();
    }
}
