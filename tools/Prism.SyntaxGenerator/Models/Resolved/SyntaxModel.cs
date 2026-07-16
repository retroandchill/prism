// @file ResolvedSyntaxModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxModel(
    ImmutableArray<SyntaxKind> kinds,
    ImmutableArray<SyntaxTrivia> trivia,
    ImmutableArray<SyntaxToken> tokens,
    ImmutableArray<SyntaxModule> modules
)
{
    public ImmutableArray<SyntaxKind> Kinds { get; } = kinds;

    public ImmutableArray<SyntaxTrivia> Trivia { get; } = trivia;

    public ImmutableArray<SyntaxToken> Tokens { get; } = tokens;

    public ImmutableArray<SyntaxModule> Modules { get; } = modules;
}
