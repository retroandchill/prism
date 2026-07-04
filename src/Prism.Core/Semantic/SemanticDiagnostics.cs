// @file SemanticDiagnostics.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Semantic;

public static class SemanticDiagnostics
{
    public static DiagnosticDescriptor DuplicateSymbolDefinition { get; } =
        new()
        {
            Id = "PRISM1001",
            Title = "Duplicate symbol definition",
            MessageFormat = "Symbol '{0}' is already defined in this scope.",
            Severity = DiagnosticSeverity.Error,
        };

    public static DiagnosticDescriptor CyclicSymbolDefinition { get; } =
        new()
        {
            Id = "PRISM1002",
            Title = "Cyclic symbol definition",
            MessageFormat = "Symbol '{0}' cannot be resolved due to a cyclic reference:\n{1}",
            Severity = DiagnosticSeverity.Error,
        };
}
