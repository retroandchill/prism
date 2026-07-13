// @file Models.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Prism.SyntaxGenerator.Models;

public sealed record SyntaxSpecification
{
    public required int Version { get; init; }
    public required ImmutableArray<ModuleDefinition> Modules { get; init; }
}

public sealed record ModuleDefinition
{
    public required string Name { get; init; }
    public required ImmutableArray<NodeDefinition> Nodes { get; init; }
}

public sealed record NodeDefinition
{
    public required string Name { get; init; }
    public string? Base { get; init; }
    public required bool IsAbstract { get; init; }
    public ImmutableArray<ChildDefinition> Children { get; init; } = [];
}

public sealed record ChildDefinition
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public bool IsOptional { get; init; }
}
