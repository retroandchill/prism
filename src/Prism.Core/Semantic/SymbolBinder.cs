// @file SymbolBinder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public sealed class SymbolBinder
{
    private readonly ConcurrentDictionary<SymbolDeclaration, Symbol> _symbolCache = new(
        ReferenceEqualityComparer.Instance
    );

    public Symbol GetSymbol(SymbolDeclaration declaration) =>
        GetSymbol(declaration, new BindingContext());

    internal Symbol GetSymbol(SymbolDeclaration declaration, BindingContext context)
    {
        if (_symbolCache.TryGetValue(declaration, out var cached))
            return cached;

        if (!context.EnterInference(declaration))
        {
            ReportCyclicDependency(declaration);
            return CreateErrorSymbol(declaration);
        }

        try
        {
            var symbol = ComputeSymbol(declaration, context);
            return _symbolCache.GetOrAdd(declaration, symbol);
        }
        finally
        {
            context.ExitInference(declaration);
        }
    }

    private Symbol ComputeSymbol(SymbolDeclaration declaration, BindingContext context)
    {
        throw new NotImplementedException();
    }

    private void ReportCyclicDependency(SymbolDeclaration declaration)
    {
        throw new NotImplementedException();
    }

    private Symbol CreateErrorSymbol(SymbolDeclaration declaration)
    {
        throw new NotImplementedException();
    }
}
