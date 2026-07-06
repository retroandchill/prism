// @file FunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using ZLinq;

namespace Prism.Core.Semantic.Symbols;

public sealed class FunctionSymbol(
    FunctionDeclarationSyntax declaration,
    Func<ParameterDeclarationSyntax, ParameterSymbol> parameterFactory,
    Symbol? containingSymbol = null
) : Symbol(declaration.Identifier.Name, declaration, containingSymbol)
{
    public override SymbolCompletionState CompletionState => Semantics.CompletionState;

    public TypeSymbol? ReturnType => Semantics.ReturnType;

    public ImmutableArray<ParameterSymbol> Parameters { get; } =
    [.. declaration.Parameters.AsValueEnumerable().Select(parameterFactory)];

    internal FunctionSymbolSemantics Semantics
    {
        get;
        set => Interlocked.Exchange(ref field, value);
    } = new();
}

internal sealed record FunctionSymbolSemantics
{
    public SymbolCompletionState CompletionState { get; init; } = SymbolCompletionState.Declared;

    public TypeSymbol? ReturnType { get; init; }
}
