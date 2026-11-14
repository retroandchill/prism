// @file ILocatableSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Semantic.Symbols.Data;

internal interface ILocatableSymbolData : ISymbolData
{
    ImmutableArray<Location> Locations { get; }
}
