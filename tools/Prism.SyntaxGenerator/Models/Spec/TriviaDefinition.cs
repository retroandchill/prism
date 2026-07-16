// @file TriviaDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;

namespace Prism.SyntaxGenerator.Models.Spec;

public readonly record struct TriviaDefinition
{
    public required string Name { get; init; }

    [UsedImplicitly]
    public string? DisplayName { get; init; }
}
