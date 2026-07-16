// @file SyntaxTypeReference.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxTypeReference
{
    public string Name { get; }

    public SyntaxNode? Definition { get; }

    internal SyntaxTypeReference(string name, SyntaxNode? definition = null)
    {
        Name = name;
        Definition = definition;
    }
}
