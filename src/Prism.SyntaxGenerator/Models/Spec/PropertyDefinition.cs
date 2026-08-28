// @file ChildDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Prism.SyntaxGenerator.Models.Spec;

[JsonConverter(typeof(JsonStringEnumConverter<PropertyShape>))]
public enum PropertyShape
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

[JsonPolymorphic]
[JsonDerivedType(typeof(PropertyItemDefinition), "item")]
[JsonDerivedType(typeof(PropertyReferenceDefinition), "reference")]
[JsonDerivedType(typeof(PropertyChoiceDefinition), "choice")]
[JsonDerivedType(typeof(PropertySequenceDefinition), "sequence")]
public abstract record PropertyDefinition;

public sealed record PropertyItemDefinition : PropertyDefinition
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required PropertyShape Shape { get; init; }
    public ImmutableArray<string> Kinds { get; init; }
    public ImmutableArray<string> Separators { get; init; }
}

public sealed record PropertyReferenceDefinition : PropertyDefinition
{
    public required string Name { get; init; }
}

public sealed record PropertyChoiceDefinition : PropertyDefinition
{
    public required ImmutableArray<PropertyDefinition> Choices { get; init; }
}

public sealed record PropertySequenceDefinition : PropertyDefinition
{
    public required ImmutableArray<PropertyDefinition> Elements { get; init; }
}
