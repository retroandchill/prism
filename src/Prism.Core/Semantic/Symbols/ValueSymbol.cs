// @file ValueSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public abstract class ValueSymbol : Symbol
{
    private protected ValueSymbol(Name name, Compilation compilation, DeclarationSyntax? declaration = null, Symbol? containingSymbol = null) : base(name, compilation, declaration, containingSymbol)
    {
    }

    public TypeSymbol Type => Compilation.SemanticModel.GetValueType(this);
}
