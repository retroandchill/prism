// @file DiagnosticDescriptor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Diagnostics;

public sealed record DiagnosticDescriptor
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string Explanation { get; init; } = "";

    public required string MessageFormat { get; init; }

    public required string Category { get; init; }

    public required DiagnosticSeverity DefaultSeverity { get; init; }

    public bool IsEnabledByDefault { get; init; } = true;

    public ImmutableArray<string> Tags { get; init; } = [];
}
