// @file IParameterSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;

namespace Prism.Semantic.Symbols.Data;

public closed record ParameterDefault;

public sealed record ConstantParameterDefault(ConstantValue Value) : ParameterDefault;

internal interface IParameterSymbolData : IValueSymbolData
{
    ParameterDefault? DefaultValue { get; }

    FunctionSymbol ContainingFunction { get; }
}
