using Prism.SyntaxGenerator.Metadata;

namespace Prism.SyntaxGenerator.Models.Cpp;

public class CppTypeReference
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

    public CppNode? Definition { get; }

    internal CppTypeReference(string name, CppNode? definition = null)
    {
        Name = name;
        Definition = definition;
    }
}
