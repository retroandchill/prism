// @file SymbolDeclarationBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public sealed class SymbolDeclarationBuilder
{
    public required Name Name { get; init; }
    public required DeclarationSyntax Syntax { get; init; }
    public DeclarationScopeBuilder? ChildScope { get; init; }

    internal SymbolDeclaration Build(DeclarationScope enclosingScope)
    {
        return new SymbolDeclaration
        {
            Name = Name,
            Syntax = Syntax,
            EnclosingScope = enclosingScope,
            ChildScope = ChildScope?.Build(enclosingScope),
        };
    }
}
