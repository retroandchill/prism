using System.Collections.Immutable;

namespace Prism.SourceGenerator.Models.CSharp;

public class CSharpSyntaxModel
{
    public ImmutableArray<CSharpKind> Kinds { get; }

    public ImmutableArray<CSharpGroup> KindGroups { get; }

    public ImmutableArray<CSharpTrivia> Trivia { get; }

    public ImmutableArray<CSharpToken> Tokens { get; }

    public ImmutableArray<CSharpModule> Modules { get; }

    public ImmutableArray<CSharpDispatchGroup> DispatchGroups { get; }

    internal CSharpSyntaxModel(
        ImmutableArray<CSharpKind> kinds,
        ImmutableArray<CSharpGroup> kindGroups,
        ImmutableArray<CSharpTrivia> trivia,
        ImmutableArray<CSharpToken> tokens,
        ImmutableArray<CSharpModule> modules,
        ImmutableArray<CSharpDispatchGroup> dispatchGroups
    )
    {
        Kinds = kinds;
        KindGroups = kindGroups;
        Trivia = trivia;
        Tokens = tokens;
        Modules = modules;
        DispatchGroups = dispatchGroups;
    }
}
