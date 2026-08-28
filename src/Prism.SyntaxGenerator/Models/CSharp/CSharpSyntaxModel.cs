using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Resolved;

namespace Prism.SyntaxGenerator.Models.CSharp;

public class CSharpSyntaxModel
{
    public ImmutableArray<CSharpKind> Kinds { get; }

    public ImmutableArray<CSharpGroup> KindGroups { get; }

    public ImmutableArray<CSharpTrivia> Trivia { get; }

    public ImmutableArray<CSharpToken> Tokens { get; }

    public ImmutableArray<CSharpModule> Modules { get; }

    public ImmutableArray<CSharpDispatchGroup> DispatchGroups { get; }

    public ImmutableArray<CSharpDiagnosticCategory> Diagnostics { get; }

    internal CSharpSyntaxModel(
        ImmutableArray<CSharpKind> kinds,
        ImmutableArray<CSharpGroup> kindGroups,
        ImmutableArray<CSharpTrivia> trivia,
        ImmutableArray<CSharpToken> tokens,
        ImmutableArray<CSharpModule> modules,
        ImmutableArray<CSharpDispatchGroup> dispatchGroups,
        ImmutableArray<CSharpDiagnosticCategory> diagnostics
    )
    {
        Kinds = kinds;
        KindGroups = kindGroups;
        Trivia = trivia;
        Tokens = tokens;
        Modules = modules;
        DispatchGroups = dispatchGroups;
        Diagnostics = diagnostics;
    }
}
