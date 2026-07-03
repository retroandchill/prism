// @file TokenKindData.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SourceGenerator.Tokens;

public readonly record struct TokenLabel(string Name, string Value);

public sealed record TokenKindData
{
    public required string Namespace { get; init; }

    public required string EnumName { get; init; }

    public required ImmutableArray<TokenLabel> Keywords { get; init; }

    public required ImmutableArray<TokenLabel> Punctuations { get; init; }
}
