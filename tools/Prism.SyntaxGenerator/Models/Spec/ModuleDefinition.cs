// @file ModuleDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Prism.SyntaxGenerator.Models.Spec;

[JsonConverter(typeof(JsonStringEnumConverter<ModuleKind>))]
public enum ModuleKind
{
    Node,
    StructuredTrivia,
}

public readonly record struct ModuleDefinition
{
    public required string Name { get; init; }

    [UsedImplicitly]
    public ModuleKind Kind { get; init; }

    public required ImmutableArray<NodeDefinition> Nodes { get; init; }
}
