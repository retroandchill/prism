// @file InternalAbiClassifier.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Prism.Semantic.Symbols;
using ZLinq;

namespace Prism.Abi;

internal sealed class InternalAbiClassifier : IAbiClassifier
{
    private InternalAbiClassifier() { }

    public static InternalAbiClassifier Instance { get; } = new();

    private const ulong MaxDirectSize = 16;

    public FunctionAbi Classify(FunctionSymbol function)
    {
        var returnTypeClassification = function.ReturnsVoid
            ? AbiValueClassification.Ignore
            : ClassifyType(function.ReturnType);

        AbiValueClassification? receiver = function.ReceiverType is not null
            ? ClassifyType(function.ReceiverType)
            : null;

        var parameterClassifications = new ParameterAbi[function.Parameters.Length];
        foreach (var (i, parameter) in function.Parameters.AsValueEnumerable().Index())
        {
            parameterClassifications[i] = new ParameterAbi(parameter, ClassifyType(parameter.Type));
        }

        return new FunctionAbi(
            function,
            receiver,
            ImmutableCollectionsMarshal.AsImmutableArray(parameterClassifications),
            new ReturnAbi(function.ReturnType, returnTypeClassification)
        );
    }

    private static AbiValueClassification ClassifyType(TypeSymbol type)
    {
        // Arrays must always be indirect
        if (type is ArrayTypeSymbol)
            return AbiValueClassification.Indirect;

        var layout = type.GetLayout();
        return layout.Size switch
        {
            0 => AbiValueClassification.Ignore,
            <= MaxDirectSize => AbiValueClassification.Direct,
            > MaxDirectSize => AbiValueClassification.Indirect,
        };
    }
}
