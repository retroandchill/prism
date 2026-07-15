// @file Generation.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models;

public sealed record GeneratedSyntaxModel
{
    public required GeneratedSyntaxKindList SyntaxKinds { get; init; }
    public required ImmutableArray<GeneratedModule> Modules { get; init; }
}

public readonly record struct Dependency(string Green, string Red);

public sealed record GeneratedModule
{
    public required string Name { get; init; }
    public required string GreenName { get; init; }
    public required string RedName { get; init; }

    public required ImmutableArray<Dependency> Imports { get; init; }
    public required ImmutableArray<Dependency> ForwardDeclarations { get; init; }
    public required ImmutableArray<GeneratedNode> Nodes { get; init; }
}

public sealed record GeneratedNode
{
    public required string Name { get; init; }
    public required string? KindName { get; init; }
    public required string GreenName { get; init; }
    public required string GreenBase { get; init; }
    public required string RedName { get; init; }
    public required string RedBase { get; init; }
    public bool IsAbstract => KindName is null;
    public required ImmutableArray<string> ChildKinds { get; init; }
    public required ImmutableArray<GeneratedChild> Children { get; init; }
}

public sealed record GeneratedChild
{
    public required string Name { get; init; }
    public required string FieldName { get; init; }
    public required string GetterName { get; init; }

    public required string GreenFieldType { get; init; }
    public required string GreenGetterType { get; init; }
    public required string GreenGetterBody { get; init; }
    public required string GetChildExpression { get; init; }
    public required string AdjustExpression { get; init; }

    public required string RedFieldType { get; init; }
    public required string RedGetterType { get; init; }

    public required bool IsOptional { get; init; }
    public required bool IsOverride { get; init; }
}

public sealed record GeneratedSyntaxKind
{
    public required string Name { get; init; }
    public required string CppName { get; init; }
    public required string DisplayName { get; init; }
    public required int Value { get; init; }
}

public sealed record GeneratedSyntaxKindGroup
{
    public required string Name { get; init; }
    public required string CppName { get; init; }
    public required ImmutableArray<GeneratedSyntaxKind> Kinds { get; init; }

    public required int StartValue { get; init; }
    public required int EndValue { get; init; }
}

public readonly record struct GroupedKeywords
{
    public required int Length { get; init; }
    public required ImmutableArray<GeneratedSyntaxKind> Keywords { get; init; }
}

public sealed class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; } = new();
    public GeneratedSyntaxKind? Value { get; set; }
}

public sealed record GeneratedTokens : IEnumerable<GeneratedSyntaxKindGroup>
{
    public required GeneratedSyntaxKindGroup Keywords { get; init; }

    public required GeneratedSyntaxKindGroup Punctuations { get; init; }
    public required GeneratedSyntaxKindGroup Others { get; init; }

    public required ImmutableArray<GroupedKeywords> KeywordsByLength { get; init; }
    public required string PunctuationTrie { get; init; }

    public IEnumerator<GeneratedSyntaxKindGroup> GetEnumerator()
    {
        yield return Keywords;
        yield return Punctuations;
        yield return Others;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public sealed record GeneratedSyntaxKindList : IEnumerable<GeneratedSyntaxKindGroup>
{
    public required GeneratedSyntaxKindGroup Trivia { get; init; }
    public required GeneratedTokens Tokens { get; init; }
    public required ImmutableArray<GeneratedSyntaxKindGroup> Nodes { get; init; }

    public IEnumerator<GeneratedSyntaxKindGroup> GetEnumerator()
    {
        yield return Trivia;
        yield return Tokens.Keywords;
        yield return Tokens.Punctuations;
        yield return Tokens.Others;

        foreach (var nodes in Nodes)
        {
            yield return nodes;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
