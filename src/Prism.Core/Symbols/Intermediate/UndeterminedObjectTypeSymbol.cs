// @file UndeterminedCollectionTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Symbols.Intermediate;

internal sealed class UndeterminedObjectTypeSymbol : IntermediateBindingTypeSymbol
{
    private UndeterminedObjectTypeSymbol()
        : base("<Undetermined Object>", NamedTypeKind.Class) { }

    public static UndeterminedObjectTypeSymbol Instance { get; } = new();
}
