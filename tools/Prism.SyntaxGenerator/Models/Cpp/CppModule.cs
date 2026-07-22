using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Cpp;

public class CppModule
{
    public string Name { get; }

    public string CppName { get; internal set; }

    public ImmutableArray<CppNode> Nodes { get; internal set; } = [];

    public IReadOnlySet<CppModule> Dependencies { get; internal set; } =
        ImmutableHashSet<CppModule>.Empty;

    public IReadOnlySet<CppNode> ForwardDeclarations { get; internal set; } =
        ImmutableHashSet<CppNode>.Empty;

    internal CppModule(string name, string cppName)
    {
        Name = name;
        CppName = cppName;
    }
}
