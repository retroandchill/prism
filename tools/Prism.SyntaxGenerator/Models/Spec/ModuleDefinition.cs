// @file ModuleDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Spec;

public readonly record struct ModuleDefinition
{
    public required string Name { get; init; }
    public required ImmutableArray<NodeDefinition> Nodes { get; init; }
}
