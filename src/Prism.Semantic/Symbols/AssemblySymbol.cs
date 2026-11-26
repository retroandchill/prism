// @file AssemblySymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class AssemblySymbol : Symbol, IMetadataReferenceProvider
{
    internal AssemblySymbol(IAssemblySymbolData data, AssemblySymbol? standardLibrary = null)
        : base(data)
    {
        StandardLibrary = standardLibrary ?? this;
    }

    internal new IAssemblySymbolData Data => Unsafe.As<IAssemblySymbolData>(base.Data);

    internal AssemblySymbol StandardLibrary { get; }

    public AssemblyIdentity Identity => Data.Identity;

    public NamespaceSymbol GlobalNamespace => Data.GlobalNamespace;

    internal NamedTypeSymbol GetSpecialType(SpecialType specialType)
    {
        return StandardLibrary.GetDeclaredSpecialType(specialType);
    }

    private NamedTypeSymbol GetDeclaredSpecialType(SpecialType specialType)
    {
        return Data.GetDeclaredSpecialType(specialType);
    }

    internal ImmutableArray<AssemblyIdentity> ReferencedAssemblies => Data.References.Identities;

    internal ImmutableArray<AssemblySymbol> ReferencedAssemblySymbols => Data.References.Symbols;

    internal ImmutableArray<UnifiedAssembly> UnifiedAssemblies => Data.References.UnifiedAssemblies;

    internal IEnumerable<AssemblySymbol> GetAllReferencedAssemblies()
    {
        return ReferencedAssemblySymbols.SelectMany(a =>
            a.GetAllReferenceAssembliesIncludingSelf()
        );
    }

    private IEnumerable<AssemblySymbol> GetAllReferenceAssembliesIncludingSelf()
    {
        yield return this;

        foreach (var assembly in GetAllReferencedAssemblies())
        {
            yield return assembly;
        }
    }

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        builder.Append(Name);
    }
}
