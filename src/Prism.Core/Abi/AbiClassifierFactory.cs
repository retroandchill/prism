// @file AbiClassifierFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;

namespace Prism.Core.Abi;

internal sealed class AbiClassifierFactory(Compilation compilation)
{
    private InternalAbiClassifier? _internalAbiClassifier;

    public IAbiClassifier Get(AbiKind kind)
    {
        switch (kind)
        {
            case AbiKind.Internal:
                Interlocked.CompareExchange(
                    ref _internalAbiClassifier,
                    new InternalAbiClassifier(compilation),
                    null
                );
                return _internalAbiClassifier;
            case AbiKind.C:
                throw new NotSupportedException("Extern C is not supported yet");
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }
}
