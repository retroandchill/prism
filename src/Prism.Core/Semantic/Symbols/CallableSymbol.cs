// @file CallableSymbol.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public abstract class CallableSymbol : Symbol
{
    public abstract TypeSymbol ReturnType { get; }
    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }
    
    private protected CallableSymbol(Name name,
        Compilation compilation,
        DeclarationSyntax? declaration = null,
        Symbol? containingSymbol = null) : base(name, compilation, declaration, containingSymbol)
    {
    }
}