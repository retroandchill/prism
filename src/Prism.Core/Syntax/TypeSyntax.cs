// @file TypeSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Syntax;

public abstract record TypeSyntax : SyntaxNode;

public sealed record BuiltInTypeSyntax : TypeSyntax
{
    public required BuiltInType Type { get; init; }
}

public sealed record NamedTypeSyntax : TypeSyntax
{
    public required IdentifierSyntax Name { get; init; }
}
