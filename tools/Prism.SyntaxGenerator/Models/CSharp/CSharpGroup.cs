using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.CSharp;

public class CSharpGroup
{
    public string Name { get; }

    public string CSharpName { get; }

    public string DisplayName { get; }

    public SyntaxGroupKind Kind { get; }

    public int StartValue { get; }

    public int EndValue { get; }

    public ImmutableArray<CSharpKind> SyntaxKinds { get; }

    internal CSharpGroup(
        string name,
        string cSharpName,
        string displayName,
        SyntaxGroupKind kind,
        ImmutableArray<CSharpKind> syntaxKinds
    )
    {
        Name = name;
        CSharpName = cSharpName;
        DisplayName = displayName;
        Kind = kind;
        StartValue = syntaxKinds[0].Value;
        EndValue = syntaxKinds[^1].Value;
        SyntaxKinds = syntaxKinds;
    }
}
