// @file BoundSymbolExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols;

namespace Prism.BoundTree.Symbols;

internal static class BoundSymbolExtensions
{
    public static BoundVariableInitializer GetBoundInitializer(
        this VariableSymbol function,
        CancellationToken cancellationToken = default
    )
    {
        if (function.Data is IBoundVariableSymbolData boundVariableSymbolData)
        {
            return boundVariableSymbolData.GetBoundInitializer(cancellationToken);
        }

        return new BoundVariableInitializer(function, null, []);
    }

    public static BoundFunctionBody GetBoundBody(
        this FunctionSymbol function,
        CancellationToken cancellationToken = default
    )
    {
        if (function.Data is IBoundFunctionSymbolData boundFunctionSymbolData)
        {
            return boundFunctionSymbolData.GetBoundBody(cancellationToken);
        }

        return new BoundFunctionBody(function, null, []);
    }
}
