// @file MemberContainerSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public closed class MemberContainerSymbol : Symbol
{
    private protected MemberContainerSymbol(ISymbolData data)
        : base(data) { }

    public ImmutableArray<Symbol> GetMembers()
    {
        return Data is IMemberContainerSymbolData container ? container.GetMembers() : [];
    }

    public ImmutableArray<Symbol> GetMembers(string name)
    {
        return Data is IMemberContainerSymbolData container ? container.GetMembers(name) : [];
    }
}
