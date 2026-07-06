// @file VariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public sealed class VariableSymbol(
    VariableDeclarationSyntax declaration,
    Symbol? containingSymbol = null
) : ValueSymbol(declaration.Identifier.Name, declaration, containingSymbol)
{
    public bool IsMutable { get; } = declaration.IsMutable;
    public override TypeSymbol? Type => Semantics.Type;

    public override SymbolCompletionState CompletionState => Semantics.CompletionState;

    internal VariableSymbolSemantics Semantics
    {
        get;
        set => Interlocked.Exchange(ref field, value);
    } = new();
}

internal sealed record VariableSymbolSemantics
{
    public SymbolCompletionState CompletionState { get; init; } = SymbolCompletionState.Declared;
    public TypeSymbol? Type { get; init; }
}
