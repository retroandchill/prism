// @file VariableSymbol.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Semantic.Binding;
using Prism.Core.Strings;

namespace Prism.Core.Semantic.Symbols;

public sealed class VariableSymbol(
    VariableDeclarationSyntax declaration,
    Compilation compilation,
    Symbol? containingSymbol = null
) : ValueSymbol(declaration.Identifier.Name, compilation, declaration, containingSymbol)
{
    public bool IsMutable { get; } = declaration.IsMutable;

    public BoundExpression? Initializer => Compilation.SemanticModel.GetVariableInitializer(this);
}
