// @file StatementSyntax.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Ast;

public abstract record StatementSyntax : SyntaxNode;

public sealed record VariableDeclarationStatementSyntax : StatementSyntax
{
    public required VariableDeclarationSyntax Declaration { get; init; }
}

public sealed record BlockSyntax : StatementSyntax, IFunctionBodySyntax
{
    public required ImmutableArray<StatementSyntax> Statements { get; init; }
}

public sealed record ReturnStatementSyntax : StatementSyntax
{
    public ExpressionSyntax? Expression { get; init; }
}

public sealed record ExpressionStatementSyntax : StatementSyntax
{
    public required ExpressionSyntax Expression { get; init; }
}

public sealed record EmptyStatementSyntax : StatementSyntax;
