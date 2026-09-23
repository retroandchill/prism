// @file IntermediateBindingTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Intermediate;

internal closed class IntermediateBindingTypeSymbol(string name, NamedTypeKind kind)
    : NamedTypeSymbol(name, null, kind)
{
    public sealed override ImmutableArray<Location> Locations => [];
    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public sealed override ImmutableArray<Symbol> GetMembers() => [];

    public sealed override ImmutableArray<Symbol> GetMembers(string name) => [];

    public sealed override bool IsDynamicallySized => false;
}
