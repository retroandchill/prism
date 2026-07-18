// @file SyntaxKindGroup.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Resolved;

public enum SyntaxGroupKind
{
    Trivia,
    Token,
    Node,
}

public sealed class SyntaxGroup
{
    public string Name { get; }

    public SyntaxGroupKind Kind { get; }

    public int StartValue { get; }

    public int EndValue { get; }

    public ImmutableArray<SyntaxKind> SyntaxKinds { get; }

    internal SyntaxGroup(string name, SyntaxGroupKind kind, ImmutableArray<SyntaxKind> syntaxKinds)
    {
        Name = name;
        StartValue = SyntaxKinds[0].Value;
        EndValue = SyntaxKinds[^1].Value;
        SyntaxKinds = syntaxKinds;
    }
}
