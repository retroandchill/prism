// @file FunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using ZLinq;

namespace Prism.Core.Semantic.Symbols;

public sealed class FunctionSymbol : CallableSymbol
{
    internal FunctionSymbol(FunctionDeclarationSyntax declaration,
        Compilation compilation,
        Func<ParameterDeclarationSyntax, FunctionSymbol, ParameterSymbol> parameterFactory,
        Symbol? containingSymbol = null) : base(declaration.Identifier.Name, compilation, declaration, containingSymbol)
    {
        Parameters = [.. declaration.Parameters.AsValueEnumerable().Select(x => parameterFactory(x, this))];
    }

    public override TypeSymbol ReturnType { get; } = null!;

    public override ImmutableArray<ParameterSymbol> Parameters { get; }
}
