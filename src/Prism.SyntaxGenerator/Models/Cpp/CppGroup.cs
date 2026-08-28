using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.Cpp;

public class CppGroup
{
    public string Name { get; }

    public string CppName { get; }

    public string DisplayName { get; }

    public SyntaxGroupKind Kind { get; }

    public int StartValue { get; }

    public int EndValue { get; }

    public ImmutableArray<CppKind> SyntaxKinds { get; }

    internal CppGroup(
        string name,
        string cppName,
        string displayName,
        SyntaxGroupKind kind,
        ImmutableArray<CppKind> syntaxKinds
    )
    {
        Name = name;
        CppName = cppName;
        DisplayName = displayName;
        Kind = kind;
        StartValue = syntaxKinds[0].Value;
        EndValue = syntaxKinds[^1].Value;
        SyntaxKinds = syntaxKinds;
    }
}
