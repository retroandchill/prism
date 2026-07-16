// @file TokenDefinitions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Prism.SyntaxGenerator.Models.Spec;

public readonly record struct TokenDefinitions
{
    public required ImmutableArray<KeywordTokenDefinition> Keywords { get; init; }
    public required ImmutableArray<PunctuationTokenDefinition> Punctuations { get; init; }
    public required ImmutableArray<OtherTokenDefinition> Other { get; init; }

    public int Length => Keywords.Length + Punctuations.Length + Other.Length;
}

public abstract record TokenDefinition
{
    public required string Name { get; init; }
}

public sealed record KeywordTokenDefinition : TokenDefinition
{
    public required string Text { get; init; }

    [UsedImplicitly]
    public bool Contextual { get; init; }
}

public sealed record PunctuationTokenDefinition : TokenDefinition
{
    public required string Text { get; init; }
}

public sealed record OtherTokenDefinition : TokenDefinition
{
    [UsedImplicitly]
    public string? DisplayName { get; init; }
}
