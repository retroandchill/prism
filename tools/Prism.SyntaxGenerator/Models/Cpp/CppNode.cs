using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Cpp;

public class CppNode : ICppSyntaxVariant
{
    public string Name { get; }

    public string GreenClassName { get; }

    public bool IsAbstract => DerivedTypes.Length > 0;

    public CppNode? Base { get; internal set; }

    public CppModule Module { get; }

    public CppKind? Kind { get; internal set; }

    public ImmutableArray<CppNode> DerivedTypes { get; internal set; }

    public ImmutableArray<CppProperty> Properties { get; internal set; }

    public ImmutableArray<CppProduction> Productions { get; internal set; }

    internal CppNode(CppModule module, string name, string greenClassName)
    {
        Module = module;
        Name = name;
        GreenClassName = greenClassName;
    }
}
