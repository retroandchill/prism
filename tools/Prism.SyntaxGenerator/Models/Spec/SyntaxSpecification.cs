// @file Models.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Spec;

public sealed record SyntaxSpecification
{
    public required int Version { get; init; }
    public required ImmutableArray<TriviaDefinition> Trivia { get; init; }
    public required TokenDefinitions Tokens { get; init; }
    public required ImmutableArray<ModuleDefinition> Modules { get; init; }
}
