// @file ResolvedSyntaxModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxModel
{
    public ImmutableArray<SyntaxKind> Kinds { get; }

    public ImmutableArray<SyntaxGroup> KindGroups { get; }

    public ImmutableArray<SyntaxTrivia> Trivia { get; }

    public ImmutableArray<SyntaxToken> Tokens { get; }

    public ImmutableArray<SyntaxModule> Modules { get; }

    internal SyntaxModel(
        ImmutableArray<SyntaxKind> kinds,
        ImmutableArray<SyntaxGroup> kindGroups,
        ImmutableArray<SyntaxTrivia> trivia,
        ImmutableArray<SyntaxToken> tokens,
        ImmutableArray<SyntaxModule> modules
    )
    {
        Kinds = kinds;
        KindGroups = kindGroups;
        Trivia = trivia;
        Tokens = tokens;
        Modules = modules;
    }
}
