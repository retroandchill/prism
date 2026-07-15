// @file Models.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prism.SyntaxGenerator.Models;

public sealed record SyntaxSpecification
{
    public required int Version { get; init; }
    public required ImmutableArray<TriviaDefinition> Trivia { get; init; }
    public required TokenDefinitions Tokens { get; init; }
    public required ImmutableArray<ModuleDefinition> Modules { get; init; }
}

public readonly record struct TriviaDefinition
{
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
}

public readonly record struct TokenDefinitions
{
    public required ImmutableArray<string> Keywords { get; init; }
    public required ImmutableArray<PunctuationTokenDefinition> Punctuations { get; init; }
    public required ImmutableArray<OtherTokenDefinition> Other { get; init; }
}

public readonly record struct PunctuationTokenDefinition
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public readonly record struct OtherTokenDefinition
{
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
}

public readonly record struct ModuleDefinition
{
    public required string Name { get; init; }
    public required ImmutableArray<NodeDefinition> Nodes { get; init; }
}

public sealed record NodeDefinition
{
    public required string Name { get; init; }
    public string? Base { get; init; }
    public required ImmutableArray<ChildDefinition> Children { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<ChildShape>))]
public enum ChildShape
{
    [JsonStringEnumMemberName("single")]
    Single,

    [JsonStringEnumMemberName("optional")]
    Optional,

    [JsonStringEnumMemberName("list")]
    List,

    [JsonStringEnumMemberName("separated_list")]
    SeparatedList,
}

public sealed record ChildDefinition
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required ChildShape Shape { get; init; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
[JsonSerializable(typeof(SyntaxSpecification))]
internal partial class ModelJsonSerializerContext : JsonSerializerContext
{
    public static JsonSerializerOptions CompositeOptions { get; }

    static ModelJsonSerializerContext()
    {
        CompositeOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        CompositeOptions.TypeInfoResolverChain.Insert(0, Default);

        CompositeOptions.MakeReadOnly();
    }
}
