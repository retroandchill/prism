using System.Collections.Generic;

namespace Prism.SourceGenerator.Models.Resolved;

public sealed class SyntaxDispatchGroup
{
    public string Name { get; }

    public bool IncludesListNode { get; }

    private readonly List<SyntaxNode> _nodes = [];
    public IReadOnlyList<SyntaxNode> Nodes => _nodes;

    internal SyntaxDispatchGroup(string name, bool includesListNode = false)
    {
        Name = name;
        IncludesListNode = includesListNode;
    }

    internal void AddNode(SyntaxNode node)
    {
        _nodes.Add(node);
    }
}
