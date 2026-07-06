// @file ErrorTypeSymbol.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public sealed class ErrorTypeSymbol(Name name, Symbol? containingSymbol = null)
    : TypeSymbol(name, null, containingSymbol)
{
    public static readonly ErrorTypeSymbol Default = new(KnownName.Error);
    public override SymbolCompletionState CompletionState => SymbolCompletionState.Error;
}