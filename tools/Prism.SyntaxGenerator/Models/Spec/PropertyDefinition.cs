// @file ChildDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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

public sealed record PropertyDefinition
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required PropertyShape Shape { get; init; }
}
