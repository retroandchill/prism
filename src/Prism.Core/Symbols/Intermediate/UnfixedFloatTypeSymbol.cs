// @file UnfixedIntegerTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Configuration;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Intermediate;

internal sealed class UnfixedFloatTypeSymbol : NamedTypeSymbol
{
    private UnfixedFloatTypeSymbol()
        : base("<Unfixed Float>", null) { }

    public static UnfixedFloatTypeSymbol Instance { get; } = new();
    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override bool IsDynamicallySized => false;

    public override ulong GetSizeInBytes(CompilationSettings settings)
    {
        throw new InvalidOperationException("Intermediate binding types don't have a size");
    }
}
