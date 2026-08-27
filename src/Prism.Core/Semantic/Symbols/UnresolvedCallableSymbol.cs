// @file ErrorFunctionSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public sealed class UnresolvedCallableSymbol : CallableSymbol
{
    public override TypeSymbol ReturnType => Compilation.SemanticModel.ErrorTypeSymbol;
    public override ImmutableArray<ParameterSymbol> Parameters => [];

    internal UnresolvedCallableSymbol(Compilation compilation)
        : base(KnownName.Error, compilation) { }
}
