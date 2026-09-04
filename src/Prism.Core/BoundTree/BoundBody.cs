// @file BoundBody.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Symbols;

namespace Prism.Core.BoundTree;

internal sealed class BoundBody(
    BoundStatement topLevelStatement,
    ImmutableHashSet<Symbol> addressedLocals
)
{
    public BoundStatement TopLevelStatement { get; } = topLevelStatement;

    public bool IsAddressTaken(Symbol symbol)
    {
        return addressedLocals.Contains(symbol);
    }
}
