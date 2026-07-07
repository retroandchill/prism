// @file DeclarationSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Ast;

public abstract record DeclarationSyntax : SyntaxNode
{
    public ImmutableArray<ModifierSyntax> Modifiers { get; init; } = [];
}

public sealed record VariableDeclarationSyntax : DeclarationSyntax
{
    public required IdentifierSyntax Identifier { get; init; }
    public TypeSyntax? Type { get; init; }
    public bool IsMutable { get; init; }
    public ExpressionSyntax? Initializer { get; init; }

}

public interface IFunctionBodySyntax;

public sealed record EmptyBody : IFunctionBodySyntax
{
    public static EmptyBody Instance { get; } = new();
}

public sealed record FunctionDeclarationSyntax : DeclarationSyntax
{
    public required IdentifierSyntax Identifier { get; init; }
    public TypeSyntax? ReturnType { get; init; }
    public ImmutableArray<ParameterDeclarationSyntax> Parameters { get; init; } = [];
    public IFunctionBodySyntax Body { get; init; } = EmptyBody.Instance;
}

public sealed record ParameterDeclarationSyntax : DeclarationSyntax
{
    public required IdentifierSyntax Identifier { get; init; }
    public required TypeSyntax Type { get; init; }
    public bool IsMutable { get; init; }
}

public sealed record EmptyDeclarationSyntax : DeclarationSyntax;
