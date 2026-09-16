// @file UnfixedIntegerTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Symbols.Intermediate;

internal sealed class UnfixedFloatTypeSymbol : IntermediateBindingTypeSymbol
{
    private UnfixedFloatTypeSymbol()
        : base("<Unfixed Float>", NamedTypeKind.Primitive) { }

    public static UnfixedFloatTypeSymbol Instance { get; } = new();
}
