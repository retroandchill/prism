// @file GeneratedDiagnostic.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Diagnostics;

namespace Prism.SourceGenerator.Diagnostics;

public sealed record FormatParam
{
    public required string Type { get; init; }
    public required string Name { get; init; }
}

public sealed record GeneratedDiagnostic
{
    public required string Namespace { get; init; }
    public required string ClassType { get; init; }
    public required string ClassName { get; init; }
    public required string Access { get; init; }
    public required string Static { get; init; }
    public required string MethodName { get; init; }
    public required bool IsExtension { get; init; }
    public required string SinkType { get; init; }
    public required string SinkParam { get; init; }
    public required string RangeParam { get; init; }
    public required string DiagnosticClass { get; init; }
    public required ImmutableArray<FormatParam> FormatParams { get; init; }
    
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string? Description { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public required string MessageFormat { get; init; }
}