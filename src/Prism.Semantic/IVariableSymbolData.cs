// @file IVariableSymbolData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Constants;

namespace Prism.Semantic;

internal interface IVariableSymbolData : IValueSymbolData
{
    VariableKind Kind { get; }

    bool HasInitializer { get; }

    ConstantValue? ConstantValue { get; }
}
