// @file IntermediateBindingTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Configuration;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Intermediate;

internal abstract class IntermediateBindingTypeSymbol(string name) : NamedTypeSymbol(name, null)
{
    public sealed override ImmutableArray<Location> Locations => [];
    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public sealed override ImmutableArray<Symbol> GetMembers() => [];

    public sealed override ImmutableArray<Symbol> GetMembers(string name) => [];

    public sealed override bool IsDynamicallySized => false;

    public sealed override SizeAndAlignment GetSizeAndAlignment(CompilationSettings settings)
    {
        throw new InvalidOperationException("Intermediate binding types don't have a size");
    }
}
