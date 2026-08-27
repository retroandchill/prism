// @file TypeSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Strings;
using Prism.Core.Syntax;

namespace Prism.Core.Semantic.Symbols;

public abstract class TypeSymbol(
    Name name,
    Compilation compilation,
    DeclarationSyntax? origin = null,
    Symbol? containingSymbol = null
) : Symbol(name, compilation, origin, containingSymbol);
