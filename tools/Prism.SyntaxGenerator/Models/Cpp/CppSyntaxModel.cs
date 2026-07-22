using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.Cpp;

public class CppSyntaxModel
{
    public ImmutableArray<CppKind> Kinds { get; }

    public ImmutableArray<CppGroup> KindGroups { get; }

    public ImmutableArray<CppTrivia> Trivia { get; }

    public ImmutableArray<CppToken> Tokens { get; }

    public ImmutableArray<CppModule> Modules { get; }

    internal CppSyntaxModel(
        ImmutableArray<CppKind> kinds,
        ImmutableArray<CppGroup> kindGroups,
        ImmutableArray<CppTrivia> trivia,
        ImmutableArray<CppToken> tokens,
        ImmutableArray<CppModule> modules
    )
    {
        Kinds = kinds;
        KindGroups = kindGroups;
        Trivia = trivia;
        Tokens = tokens;
        Modules = modules;
    }
}
