// @file FunctionAbi.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;

namespace Prism.Core.Abi;

internal readonly record struct ParameterAbi(
    ParameterSymbol Parameter,
    AbiValueClassification Classification
);

internal readonly record struct ReturnAbi(TypeSymbol Type, AbiValueClassification Classification);

internal sealed record FunctionAbi(
    FunctionSymbol Function,
    ImmutableArray<ParameterAbi> Parameters,
    ReturnAbi Return
);
