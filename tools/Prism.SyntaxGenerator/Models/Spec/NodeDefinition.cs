// @file NodeDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Prism.SyntaxGenerator.Models.Spec;

public sealed record NodeDefinition
{
    public required string Name { get; init; }

    [UsedImplicitly]
    public string? Base { get; init; }

    [UsedImplicitly]
    public ImmutableArray<PropertyDefinition> Properties { get; init; } = [];
}
