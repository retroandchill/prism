// @file MissingAssemblySymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Configuration;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;

namespace Prism.Compiler.Symbols;

internal sealed class MissingAssemblySymbol : IAssemblySymbolData
{
    public MissingAssemblySymbol(AssemblyIdentity identity, TargetInfo targetInfo)
    {
        Identity = identity;
        TargetInfo = targetInfo;
        PublicSymbol = new AssemblySymbol(this);
        GlobalNamespace = new MissingGlobalNamespace(PublicSymbol).PublicSymbol;
    }

    public AssemblySymbol PublicSymbol { get; }

    public string Name => Identity.Name;
    public Symbol? ContainingSymbol => null;
    public AssemblyIdentity Identity { get; }
    public TargetInfo TargetInfo { get; }
    public AssemblyReferences References => AssemblyReferences.Empty;
    public NamespaceSymbol GlobalNamespace { get; }
}

internal sealed class MissingGlobalNamespace : INamespaceSymbolData
{
    public MissingGlobalNamespace(AssemblySymbol containingAssembly)
    {
        ContainingAssembly = containingAssembly;
        PublicSymbol = new NamespaceSymbol(this);
    }

    public NamespaceSymbol PublicSymbol { get; }

    public string Name => CommonNames.Global;
    public Symbol ContainingSymbol => ContainingAssembly;
    public AssemblySymbol ContainingAssembly { get; }
    public TypeSymbol? ContainingType => null;
    public NamespaceSymbol? ContainingNamespace => null;
    public MemberContainerSymbol? ContainingMemberContainer => null;

    public ImmutableArray<Symbol> GetMembers() => [];

    public ImmutableArray<Symbol> GetMembers(string name) => [];

    public NamespaceKind NamespaceKind => NamespaceKind.Assembly;
}
