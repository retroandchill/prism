// @file IntermediateBindingTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Intermediate;

internal closed class IntermediateBindingTypeSymbol : NamedTypeSymbol
{
    public sealed override Symbol? ContainingSymbol => null;

    public sealed override ImmutableArray<Location> Locations => [];
    public sealed override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override bool IsDefaultConstructible => false;

    public sealed override ImmutableArray<Symbol> GetMembers() => [];

    public sealed override ImmutableArray<Symbol> GetMembers(string name) => [];

    public sealed override bool IsDynamicallySized => false;
}
