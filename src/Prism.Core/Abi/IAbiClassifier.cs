// @file IAbiClassifier.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Symbols;

namespace Prism.Core.Abi;

internal interface IAbiClassifier
{
    FunctionAbi Classify(FunctionSymbol function);
}
