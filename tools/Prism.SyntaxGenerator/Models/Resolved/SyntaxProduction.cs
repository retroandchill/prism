// @file SyntaxProduction.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxProduction : ISyntaxVariant
{
    public string Name { get; }

    public SyntaxNode Node { get; }

    public SyntaxKind? Kind { get; internal set; }

    public ImmutableArray<SyntaxProductionConstraint> Constraints { get; }

    internal SyntaxProduction(
        string name,
        SyntaxNode node,
        ImmutableArray<SyntaxProductionConstraint> constraints
    )
    {
        Name = name;
        Node = node;
        Constraints = constraints;
    }
}
