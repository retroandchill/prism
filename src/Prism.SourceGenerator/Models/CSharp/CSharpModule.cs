using System.Collections.Generic;
using System.Collections.Immutable;
using Prism.SourceGenerator.Models.Spec;

namespace Prism.SourceGenerator.Models.CSharp;

public class CSharpModule
{
    public string Name { get; }

    public string CSharpName { get; internal set; }

    public ModuleKind Kind { get; }

    public ImmutableArray<CSharpNode> Nodes { get; internal set; } = [];

    public IReadOnlyCollection<CSharpModule> Dependencies { get; internal set; } =
        ImmutableHashSet<CSharpModule>.Empty;

    public IReadOnlyCollection<CSharpNode> ForwardDeclarations { get; internal set; } =
        ImmutableHashSet<CSharpNode>.Empty;

    internal CSharpModule(string name, string cSharpName, ModuleKind kind)
    {
        Name = name;
        CSharpName = cSharpName;
        Kind = kind;
    }
}
