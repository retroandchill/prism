// @file NamedTypeSymbol.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public sealed class NamedTypeSymbol : TypeSymbol
{
    public BuiltInType? BuiltInType { get; }

    public override SymbolCompletionState CompletionState { get; }

    internal NamedTypeSymbol(BuiltInType type) : base(KnownName.From(type))
    {
        BuiltInType = type;
        CompletionState = SymbolCompletionState.Bound;
    }
}