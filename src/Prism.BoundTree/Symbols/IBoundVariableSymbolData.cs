// @file IBoundVariableSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Semantic.Symbols.Data;

namespace Prism.BoundTree.Symbols;

internal interface IBoundVariableSymbolData : IVariableSymbolData
{
    BoundVariableInitializer GetBoundInitializer(CancellationToken cancellationToken = default);
}
