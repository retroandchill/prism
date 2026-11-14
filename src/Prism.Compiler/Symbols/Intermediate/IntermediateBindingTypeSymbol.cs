// @file IntermediateBindingTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Diagnostics;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Intermediate;

internal closed class IntermediateBindingTypeSymbol : INamedTypeSymbolData
{
    public abstract string Name { get; }
    public abstract NamedTypeKind Kind { get; }
    public Symbol? ContainingSymbol => null;

    public bool IsDefaultConstructible => false;

    public bool IsDynamicallySized => false;

    public NamedTypeSymbol PublicSymbol { get; }

    protected IntermediateBindingTypeSymbol()
    {
        PublicSymbol = new NamedTypeSymbol(this);
    }
}
