// @file SyntaxChild.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxProperty
{
    public string Name { get; }
    public SyntaxTypeReference Type { get; }
    public PropertyShape Shape { get; internal set; }
    public bool IsOverride { get; internal set; }

    internal SyntaxProperty(string name, SyntaxTypeReference type, PropertyShape shape)
    {
        Name = name;
        Type = type;
        Shape = shape;
    }
}
