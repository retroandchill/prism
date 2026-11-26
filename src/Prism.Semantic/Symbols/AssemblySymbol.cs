// @file AssemblySymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Text;
using Prism.Configuration;
using Prism.Semantic.Symbols.Data;
using Prism.Utils;

namespace Prism.Semantic.Symbols;

public sealed class AssemblySymbol : Symbol, IMetadataReferenceProvider
{
    internal AssemblySymbol(IAssemblySymbolData data)
        : base(data) { }

    internal new IAssemblySymbolData Data => Unsafe.As<IAssemblySymbolData>(base.Data);

    internal AssemblySymbol StandardLibrary => Data.StandardLibrary;

    public AssemblyIdentity Identity => Data.Identity;

    public TargetInfo TargetInfo => Data.TargetInfo;

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
        return ImmutableCollectionsMarshal
            .AsArray(ReferencedAssemblySymbols)
            .NullToEmpty()
            .SelectMany(a => a.GetAllReferenceAssembliesIncludingSelf());
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
