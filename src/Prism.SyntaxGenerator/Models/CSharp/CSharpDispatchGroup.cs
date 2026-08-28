using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.CSharp;

public sealed class CSharpDispatchGroup
{
    public string Name { get; }

    public string GreenClassName { get; }

    public string RedClassName { get; }

    public bool IncludesListNode { get; }

    public ImmutableArray<CSharpNode> Nodes { get; }

    internal CSharpDispatchGroup(
        string name,
        string greenClassName,
        string redClassName,
        bool includesListNode,
        ImmutableArray<CSharpNode> nodes
    )
    {
        Name = name;
        GreenClassName = greenClassName;
        RedClassName = redClassName;
        IncludesListNode = includesListNode;
        Nodes = nodes;
    }
}
