using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.CSharp;

public class CSharpNode : ICSharpSyntaxVariant
{
    public string Name { get; }

    public string GreenClassName { get; }

    public string RedClassName { get; }

    public string CSharpVariableName { get; }

    public bool IsAbstract => DerivedTypes.Length > 0;

    public CSharpNode? Base { get; internal set; }

    public CSharpModule Module { get; }

    public CSharpKind? Kind { get; internal set; }

    public ImmutableArray<CSharpNode> DerivedTypes { get; internal set; }

    public ImmutableArray<CSharpProperty> Properties { get; internal set; }

    public ImmutableArray<CSharpProduction> Productions { get; internal set; }

    internal CSharpNode(
        CSharpModule module,
        string name,
        string greenClassName,
        string redClassName,
        string csharpVariableName
    )
    {
        Module = module;
        Name = name;
        GreenClassName = greenClassName;
        RedClassName = redClassName;
        CSharpVariableName = csharpVariableName;
    }
}
