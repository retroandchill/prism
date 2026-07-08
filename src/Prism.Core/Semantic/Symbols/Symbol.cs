// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Strings;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic.Symbols;

public abstract class Symbol
{
    private protected Symbol(
        Name name,
        Compilation compilation,
        DeclarationSyntax? declaration = null,
        Symbol? containingSymbol = null
    )
    {
        Name = name;
        Compilation = compilation;
        Declaration = declaration;
        ContainingSymbol = containingSymbol;
    }

    public Name Name { get; }

    public DeclarationSyntax? Declaration { get; }

    public Symbol? ContainingSymbol { get; }

    public Compilation Compilation { get; }
}
