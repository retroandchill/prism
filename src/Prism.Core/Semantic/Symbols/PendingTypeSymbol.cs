// @file ErrorTypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public sealed class PendingTypeSymbol(Compilation compilation, Symbol? containingSymbol = null)
    : TypeSymbol(KnownName.Pending, compilation, containingSymbol: containingSymbol);
