// @file IBoundSemanticContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Configuration;
using Prism.Semantic.Layout;
using Prism.Semantic.Symbols;

namespace Prism.BoundTree;

internal interface IBoundSemanticContext
{
    PointerWidth PointerWidth { get; }

    NamedTypeSymbol GetSpecialType(SpecialType specialType);
}
