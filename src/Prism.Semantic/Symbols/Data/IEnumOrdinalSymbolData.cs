// @file IEnumOrdinalSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;

namespace Prism.Semantic.Symbols.Data;

internal interface IEnumOrdinalSymbolData : ISymbolData
{
    public ConstantValue Value { get; }
}
