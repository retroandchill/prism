// @file CompilationUnitSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Ast;

public sealed record CompilationUnitSyntax : SyntaxNode
{
    public required ImmutableArray<DeclarationSyntax> Declarations { get; init; }
}
