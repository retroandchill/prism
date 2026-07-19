// @file SyntaxChild.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class SyntaxProperty
{
    public SyntaxNode Owner { get; }
    public string Name { get; }
    public SyntaxTypeReference Type { get; }
    public PropertyShape Shape { get; internal set; }

    [MemberNotNullWhen(true, nameof(OverrideOf))]
    public bool IsOverride => OverrideOf is not null;
    public SyntaxProperty? OverrideOf { get; internal set; }

    internal SyntaxProperty(
        SyntaxNode owner,
        string name,
        SyntaxTypeReference type,
        PropertyShape shape
    )
    {
        Owner = owner;
        Name = name;
        Type = type;
        Shape = shape;
    }
}
