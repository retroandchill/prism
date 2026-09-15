// @file UnboundNullType.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Symbols.Intermediate;

internal sealed class UnboundNullTypeSymbol : IntermediateBindingTypeSymbol
{
    private UnboundNullTypeSymbol()
        : base("<Null>") { }

    public static UnboundNullTypeSymbol Instance { get; } = new();
}
