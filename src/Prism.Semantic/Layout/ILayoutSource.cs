// @file ILayoutSource.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Configuration;
using Prism.Semantic.Symbols;

namespace Prism.Semantic.Layout;

public interface ILayoutSource
{
    PointerWidth PointerWidth { get; }

    TypeLayout GetTypeLayout(TypeSymbol typeSymbol, CancellationToken cancellationToken = default);
}
