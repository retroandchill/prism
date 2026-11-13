// @file AssemblySymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Cysharp.Text;

namespace Prism.Semantic;

public sealed class AssemblySymbol : Symbol, IMetadataReferenceProvider
{
    internal AssemblySymbol(IAssemblySymbolData data)
        : base(data) { }

    internal new IAssemblySymbolData Data => Unsafe.As<IAssemblySymbolData>(base.Data);

    public AssemblyIdentity Identity => Data.Identity;

    public NamespaceSymbol GlobalNamespace => Data.GlobalNamespace;

    internal ImmutableArray<AssemblyIdentity> ReferencedAssemblies => Data.References.Identities;

    internal ImmutableArray<AssemblySymbol> ReferencedAssemblySymbols => Data.References.Symbols;

    internal ImmutableArray<UnifiedAssembly> UnifiedAssemblies => Data.References.UnifiedAssemblies;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        builder.Append(Name);
    }
}
