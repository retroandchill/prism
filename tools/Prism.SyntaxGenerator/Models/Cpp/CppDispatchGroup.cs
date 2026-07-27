using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppDispatchGroup
{
    public string Name { get; }

    public string GreenClassName { get; }

    public string RedClassName { get; }

    public bool IncludesListNode { get; }

    public ImmutableArray<CppNode> Nodes { get; }

    internal CppDispatchGroup(
        string name,
        string greenClassName,
        string redClassName,
        bool includesListNode,
        ImmutableArray<CppNode> nodes
    )
    {
        Name = name;
        GreenClassName = greenClassName;
        RedClassName = redClassName;
        IncludesListNode = includesListNode;
        Nodes = nodes;
    }
}
