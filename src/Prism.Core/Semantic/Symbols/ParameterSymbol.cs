// @file ParameterSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;

namespace Prism.Core.Semantic.Symbols;

public sealed class ParameterSymbol(
    ParameterDeclarationSyntax declaration,
    Compilation compilation,
    Symbol? containingSymbol = null
) : ValueSymbol(declaration.Identifier.Name, compilation, declaration, containingSymbol)
{
    public bool IsMutable { get; } = declaration.IsMutable;
}
