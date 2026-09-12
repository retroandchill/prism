// @file FunctionAbi.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Symbols;

namespace Prism.Core.Abi;

internal readonly record struct ParameterAbi(
    ParameterSymbol Parameter,
    AbiValueClassification Classification
)
{
    public bool IsIndirect => Classification == AbiValueClassification.Indirect;
}

internal readonly record struct ReturnAbi(TypeSymbol Type, AbiValueClassification Classification)
{
    public bool IsIndirect => Classification == AbiValueClassification.Indirect;
}

internal sealed record FunctionAbi(
    FunctionSymbol Function,
    ImmutableArray<ParameterAbi> Parameters,
    ReturnAbi Return
)
{
    public bool IsParameterIndirect(ParameterSymbol parameter)
    {
        Debug.Assert(parameter.ContainingFunction == Function);
        var parameterAbi = Parameters.First(p => p.Parameter == parameter);
        return parameterAbi.IsIndirect;
    }
}
