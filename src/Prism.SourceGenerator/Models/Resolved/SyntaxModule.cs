// @file ResolvedModule.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Prism.SourceGenerator.Models.Spec;

namespace Prism.SourceGenerator.Models.Resolved;

public sealed class SyntaxModule
{
    private readonly List<SyntaxNode> _nodes = [];
    private readonly HashSet<SyntaxModule> _dependencies = [];
    private readonly HashSet<SyntaxNode> _forwardDeclarations = [];
    public string Name { get; }

    public ModuleKind Kind { get; }

    public IReadOnlyList<SyntaxNode> Nodes => _nodes;

    public IReadOnlyCollection<SyntaxModule> Dependencies => _dependencies;

    public IReadOnlyCollection<SyntaxNode> ForwardDeclarations => _forwardDeclarations;

    internal SyntaxModule(string name, ModuleKind kind)
    {
        Name = name;
        Kind = kind;
    }

    internal void AddNode(SyntaxNode node)
    {
        _nodes.Add(node);
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
