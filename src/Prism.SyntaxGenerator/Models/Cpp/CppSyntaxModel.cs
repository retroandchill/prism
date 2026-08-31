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

    public ImmutableArray<CppDispatchGroup> DispatchGroups { get; }

    public ImmutableArray<CppDiagnosticCategory> Diagnostics { get; }

    public ImmutableArray<CppBoundNode> BoundNodes { get; }

    internal CppSyntaxModel(
        ImmutableArray<CppKind> kinds,
        ImmutableArray<CppGroup> kindGroups,
        ImmutableArray<CppTrivia> trivia,
        ImmutableArray<CppToken> tokens,
        ImmutableArray<CppModule> modules,
        ImmutableArray<CppDispatchGroup> dispatchGroups,
        ImmutableArray<CppDiagnosticCategory> diagnostics,
        ImmutableArray<CppBoundNode> boundNodes
    )
    {
        Kinds = kinds;
        KindGroups = kindGroups;
        Trivia = trivia;
        Tokens = tokens;
        Modules = modules;
        DispatchGroups = dispatchGroups;
        Diagnostics = diagnostics;
        BoundNodes = boundNodes;
    }
}
