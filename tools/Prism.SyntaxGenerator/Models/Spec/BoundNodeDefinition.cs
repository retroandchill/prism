// @file BoundNodeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Spec;

public sealed record BoundNodeDefinition
{
    public required string Name { get; init; }
    public bool HasModule { get; init; } = false;
    public ImmutableArray<BoundNodeDefinition> Children { get; init; } = [];
}
