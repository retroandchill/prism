// @file AbiClassifierFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Abi;

internal static class AbiClassifierFactory
{
    public static IAbiClassifier Get(AbiKind kind)
    {
        return kind switch
        {
            AbiKind.Internal => InternalAbiClassifier.Instance,
            AbiKind.C => throw new NotSupportedException("Extern C is not supported yet"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }
}
