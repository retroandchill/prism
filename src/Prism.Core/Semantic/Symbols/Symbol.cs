// @file Symbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

#pragma warning disable CA1067
#pragma warning disable CA1050

namespace Prism.Core.Semantic.Symbols;

public closed class Symbol(Name name, DeclarationSyntax? declaration = null, Symbol? containingSymbol = null)
{
    public Name Name { get; } = name;
    
    public DeclarationSyntax? Declaration { get; } = declaration;
    
    public Symbol? ContainingSymbol { get; } = containingSymbol;
    
    public abstract SymbolCompletionState CompletionState { get; }
    
    public bool IsBound => CompletionState == SymbolCompletionState.Bound;
    
    public bool HasErrors => CompletionState == SymbolCompletionState.Error;
}