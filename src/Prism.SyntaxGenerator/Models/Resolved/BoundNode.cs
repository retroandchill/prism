// @file BoundNode.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class BoundNode
{
    public string Name { get; }

    public bool HasModule { get; }

    private readonly List<BoundNode> _leafNodes = [];
    public IReadOnlyList<BoundNode> LeafNodes => _leafNodes;

    internal BoundNode(string name, bool hasModule)
    {
        Name = name;
        HasModule = hasModule;
    }

    internal void AddLeafNode(BoundNode node)
    {
        _leafNodes.Add(node);
    }
}
