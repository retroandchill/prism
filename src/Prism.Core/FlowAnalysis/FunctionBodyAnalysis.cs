// @file FunctionBodyAnalysis.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.BoundTree;
using Prism.Core.Compiling;
using Prism.Core.Symbols;

namespace Prism.Core.FlowAnalysis;

internal sealed class FunctionBodyAnalysis
{
    private readonly ImmutableHashSet<Symbol> _addressTakenSymbols;

    public FunctionBodyAnalysis(ImmutableHashSet<Symbol> addressTakenSymbols)
    {
        _addressTakenSymbols = addressTakenSymbols;
    }

    public bool IsAddressTaken(Symbol symbol)
    {
        return _addressTakenSymbols.Contains(symbol);
    }
}
