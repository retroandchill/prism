namespace Prism.SyntaxGenerator.Models.CSharp;

public sealed class CSharpTrivia : ICSharpSyntaxVariant
{
    public string Name { get; }

    public string? DisplayName { get; }

    public CSharpKind Kind { get; internal set; } = null!;

    internal CSharpTrivia(string name, string? displayName = null)
    {
        Name = name;
        DisplayName = displayName;
    }
}
