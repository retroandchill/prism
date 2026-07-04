// @file SymbolDeclaration.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public sealed record SymbolDeclaration
{
    public required Name Name { get; init; }
    public required DeclarationSyntax Syntax { get; init; }
    public required DeclarationScope EnclosingScope { get; init; }
    public DeclarationScope? ChildScope { get; init; }
}
