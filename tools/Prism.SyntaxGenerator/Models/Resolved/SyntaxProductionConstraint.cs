// @file Resolved.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public closed class SyntaxProductionConstraint
{
    public SyntaxProperty Property { get; }

    private protected SyntaxProductionConstraint(SyntaxProperty property)
    {
        Property = property;
    }
}

public sealed class SyntaxTokenConstraint : SyntaxProductionConstraint
{
    public SyntaxToken Token { get; }

    internal SyntaxTokenConstraint(SyntaxProperty property, SyntaxToken token) : base(property)
    {
        Token = token;
    }
}

public sealed class SyntaxPropertyPresenceConstraint : SyntaxProductionConstraint
{
    public bool Required { get; }

    internal SyntaxPropertyPresenceConstraint(SyntaxProperty property, bool required) : base(property)
    {
        Required = required;
    }
}
