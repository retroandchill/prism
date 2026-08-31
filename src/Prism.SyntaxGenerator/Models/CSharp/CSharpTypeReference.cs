using Prism.SyntaxGenerator.Metadata;

namespace Prism.SyntaxGenerator.Models.CSharp;

public class CSharpTypeReference
{
    public string Name { get; }

    public string GreenClassName
    {
        get
        {
            if (Definition is not null)
                return Definition.GreenClassName;

            return Name switch
            {
                "Node" => CommonNames.GreenNodeClass,
                "Token" => CommonNames.GreenTokenClass,
                _ => throw new InvalidOperationException($"Unknown type reference: {Name}"),
            };
        }
    }

    public string RedClassName
    {
        get
        {
            if (Definition is not null)
                return Definition.RedClassName;

            return Name switch
            {
                "Node" => CommonNames.SyntaxNodeClass,
                _ => throw new InvalidOperationException($"Unknown type reference: {Name}"),
            };
        }
    }

    public CSharpNode? Definition { get; }

    internal CSharpTypeReference(string name, CSharpNode? definition = null)
    {
        Name = name;
        Definition = definition;
    }
}
