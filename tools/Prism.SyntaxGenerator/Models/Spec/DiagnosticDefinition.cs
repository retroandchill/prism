// @file DiagnosticDefinition.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Prism.SyntaxGenerator.Models.Spec;

[JsonConverter(typeof(JsonStringEnumConverter<DiagnosticSeverity>))]
public enum DiagnosticSeverity
{
    Error,
    Warning,
    Info,
    Hint,
}

public record DiagnosticArgumentDefinition(string Name, string Type);

public sealed record DiagnosticDefinition
{
    public required string Name { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required string Format { get; init; }

    [UsedImplicitly]
    public ImmutableArray<DiagnosticArgumentDefinition> Args { get; init; } = [];

    [UsedImplicitly]
    public string Explanation { get; init; } = "";
}
