// @file UndeterminedCollectionTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Metadata;

namespace Prism.Compiler.Symbols.Intermediate;

internal sealed class UndeterminedCollectionTypeSymbol : IntermediateBindingTypeSymbol
{
    public static UndeterminedCollectionTypeSymbol Instance { get; } = new();
    public override string Name => "<Undetermined Collection>";
    public override NamedTypeKind Kind => NamedTypeKind.Class;
}
