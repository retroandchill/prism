// @file Resolved.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models;

public sealed class ResolvedSyntaxModel(
    ImmutableArray<TriviaDefinition> trivia,
    TokenDefinitions tokens,
    IReadOnlyDictionary<string, ResolvedModule> modules,
    IReadOnlyDictionary<string, SyntaxNode> nodes
)
{
    public ImmutableArray<TriviaDefinition> Trivia { get; } = trivia;

    public TokenDefinitions Tokens { get; } = tokens;

    public IReadOnlyDictionary<string, ResolvedModule> Modules { get; } = modules;

    public IReadOnlyDictionary<string, SyntaxNode> Nodes { get; } = nodes;

    public IEnumerable<SyntaxNode> RootNodes => Nodes.Values.Where(n => n.Base is null);
}

public sealed class ResolvedModule
{
    public required string Name { get; init; }

    public List<SyntaxNode> Nodes { get; } = [];

    public HashSet<ResolvedModule> Dependencies { get; } = [];

    public HashSet<SyntaxNode> ForwardDeclarations { get; } = [];
}

public sealed class SyntaxNode
{
    public string Name => Definition.Name;

    public required NodeDefinition Definition { get; init; }

    public bool IsAbstract => DerivedTypes.Count > 0;

    public SyntaxNode? Base { get; set; }

    public required ResolvedModule Module { get; init; }

    public List<SyntaxNode> DerivedTypes { get; } = [];

    public List<SyntaxChild> Children { get; } = [];
}

public sealed class SyntaxChild
{
    public required string Name { get; init; }
    public required SyntaxTypeReference Type { get; init; }
    public required ChildShape Shape { get; init; }
    public bool IsOverride { get; set; }
}

public sealed class SyntaxTypeReference
{
    public required string Name { get; init; }

    public SyntaxNode? Definition { get; init; }

    public bool IsBuiltIn => Definition is null;
}
