using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppToken : ICppSyntaxVariant
{
    public string Name { get; }

    public TokenCategory Category { get; }

    public CppKind Kind { get; internal set; } = null!;

    public TokenFlags Flags { get; internal init; }

    public bool Contextual => Flags.HasFlag(TokenFlags.Contextual);

    public string? Text { get; internal init; }

    public string? DisplayName { get; internal init; }

    internal CppToken(string name, TokenCategory category)
    {
        Name = name;
        Category = category;
    }
}
