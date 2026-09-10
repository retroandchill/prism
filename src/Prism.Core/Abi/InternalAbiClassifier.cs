// @file InternalAbiClassifier.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Prism.Core.Compiling;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Abi;

internal sealed class InternalAbiClassifier(Compilation compilation) : IAbiClassifier
{
    private const ulong MaxDirectSize = 16;

    public FunctionAbi Classify(FunctionSymbol function)
    {
        var returnTypeClassification = function.ReturnsVoid
            ? AbiValueClassification.Ignore
            : ClassifyType(function.ReturnType);

        var parameterClassifications = new ParameterAbi[function.Parameters.Length];
        foreach (var (i, parameter) in function.Parameters.AsValueEnumerable().Index())
        {
            parameterClassifications[i] = new ParameterAbi(parameter, ClassifyType(parameter.Type));
        }

        return new FunctionAbi(
            function,
            ImmutableCollectionsMarshal.AsImmutableArray(parameterClassifications),
            new ReturnAbi(function.ReturnType, returnTypeClassification)
        );
    }

    private AbiValueClassification ClassifyType(TypeSymbol type)
    {
        // Arrays must always be indirect
        if (type is ArrayTypeSymbol)
            return AbiValueClassification.Indirect;

        var size = type.GetSizeInBytes(compilation.Settings);
        return size switch
        {
            0 => AbiValueClassification.Ignore,
            <= MaxDirectSize => AbiValueClassification.Direct,
            > MaxDirectSize => AbiValueClassification.Indirect,
        };
    }
}
