using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.CSharp;

public class CSharpModule
{
    public string Name { get; }

    public string CSharpName { get; internal set; }

    public ModuleKind Kind { get; }

    public ImmutableArray<CSharpNode> Nodes { get; internal set; } = [];

    public IReadOnlySet<CSharpModule> Dependencies { get; internal set; } =
        ImmutableHashSet<CSharpModule>.Empty;

    public IReadOnlySet<CSharpNode> ForwardDeclarations { get; internal set; } =
        ImmutableHashSet<CSharpNode>.Empty;

    internal CSharpModule(string name, string cSharpName, ModuleKind kind)
    {
        Name = name;
        CSharpName = cSharpName;
        Kind = kind;
    }
}
