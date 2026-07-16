// @file SyntaxKind.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public union SyntaxVariant(SyntaxTrivia, SyntaxToken, SyntaxNode, SyntaxProduction);

public class SyntaxKind
{
    public string Name { get; }

    public int Value { get; }

    public SyntaxVariant Data { get; }

    internal SyntaxKind(string name, int value, SyntaxVariant data)
    {
        Name = name;
        Value = value;
        Data = data;
    }
}
