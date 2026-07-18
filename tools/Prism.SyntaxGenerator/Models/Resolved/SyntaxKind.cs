// @file SyntaxKind.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public interface ISyntaxVariant;

public sealed class SyntaxKind
{
    public string Name { get; }

    public int Value { get; }

    public ISyntaxVariant Data { get; }

    internal SyntaxKind(string name, int value, ISyntaxVariant data)
    {
        Name = name;
        Value = value;
        Data = data;
    }
}
