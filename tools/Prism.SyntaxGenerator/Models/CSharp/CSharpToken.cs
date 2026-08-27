using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.CSharp;

public sealed class CSharpToken : ICSharpSyntaxVariant
{
    public string Name { get; }

    public TokenCategory Category { get; }

    public CSharpKind Kind { get; internal set; } = null!;

    public TokenFlags Flags { get; internal init; }

    public bool Contextual => Flags.HasFlag(TokenFlags.Contextual);

    public string? Text { get; internal init; }

    public string? DisplayName { get; internal init; }

    internal CSharpToken(string name, TokenCategory category)
    {
        Name = name;
        Category = category;
    }
}
