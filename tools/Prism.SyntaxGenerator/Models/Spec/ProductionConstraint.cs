// @file ProductionConstraint.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Prism.SyntaxGenerator.Models.Spec;

[JsonPolymorphic]
[JsonDerivedType(typeof(TokenConstraint), "token")]
[JsonDerivedType(typeof(PropertyPresenceConstraint), "property")]
public abstract record ProductionConstraint
{
    public required string Property { get; init; }
}

public sealed record TokenConstraint : ProductionConstraint
{
    public required string Token { get; init; }
}

public sealed record PropertyPresenceConstraint : ProductionConstraint
{
    public required bool Required { get; init; }
}
