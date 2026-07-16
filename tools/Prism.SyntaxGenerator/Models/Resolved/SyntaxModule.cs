// @file ResolvedModule.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxModule
{
    private readonly List<SyntaxNode> _nodes = [];
    private readonly HashSet<SyntaxModule> _dependencies = [];
    private readonly HashSet<SyntaxNode> _forwardDeclarations = [];
    public required string Name { get; init; }

    public IReadOnlyList<SyntaxNode> Nodes => _nodes;

    public IReadOnlySet<SyntaxModule> Dependencies => _dependencies;

    public IReadOnlySet<SyntaxNode> ForwardDeclarations => _forwardDeclarations;

    internal void AddNode(SyntaxNode node)
    {
        _nodes.Add(node);
    }

    internal void EnsureCapacity(int capacity)
    {
        _nodes.EnsureCapacity(capacity);
    }

    internal void AddDependency(SyntaxModule dependency)
    {
        _dependencies.Add(dependency);
    }

    internal void AddForwardDeclaration(SyntaxNode declaration)
    {
        _forwardDeclarations.Add(declaration);
    }
}
