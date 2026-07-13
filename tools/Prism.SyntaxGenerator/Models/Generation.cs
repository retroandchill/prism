// @file Generation.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models;

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
    public required string GreenName { get; init; }
    public required string GreenBase { get; init; }
    public required string RedName { get; init; }
    public required string RedBase { get; init; }
    public required bool IsAbstract { get; init; }
    public required ImmutableArray<GeneratedChild> Children { get; init; }
}

public sealed record GeneratedChild
{
    public required string Name { get; init; }
    public required string FieldName { get; init; }
    public required string GetterName { get; init; }

    public required string GreenFieldType { get; init; }
    public required string GreenGetterType { get; init; }

    public required string RedFieldType { get; init; }
    public required string RedGetterType { get; init; }

    public required bool IsOptional { get; init; }
    public required bool IsOverride { get; init; }
}
