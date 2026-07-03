// @file TypeSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable CA1067

namespace Prism.Core.Ast;

public closed record TypeSyntax : SyntaxNode;

public sealed record BuiltInTypeSyntax : TypeSyntax
{
    public required BuiltInType Type { get; init; }
}

public sealed record NamedTypeSyntax : TypeSyntax
{
    public required IdentifierSyntax Name { get; init; }
}
