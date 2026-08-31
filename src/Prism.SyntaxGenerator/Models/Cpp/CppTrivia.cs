namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppTrivia : ICppSyntaxVariant
{
    public string Name { get; }

    public string? DisplayName { get; }

    public CppKind Kind { get; internal set; } = null!;

    internal CppTrivia(string name, string? displayName = null)
    {
        Name = name;
        DisplayName = displayName;
    }
}
