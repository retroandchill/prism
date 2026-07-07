// @file BoundStatement.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Semantic.Binding;

public abstract class BoundStatement : BoundNode;

public sealed class BoundBlock : BoundStatement
{
    public required ImmutableArray<BoundStatement> Statements { get; init; }
}

public sealed class BoundLocalVariable : BoundStatement
{
    public required VariableSymbol Symbol { get; init; }

    public required BoundExpression? Initializer { get; init; }
}

public sealed class BoundExpressionStatement : BoundStatement
{
    public required BoundExpression Expression { get; init; }
}

public sealed class BoundReturnStatement : BoundStatement
{
    public required BoundExpression? Expression { get; init; }
}
