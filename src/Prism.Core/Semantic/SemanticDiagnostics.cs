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

    public static DiagnosticDescriptor VariableWithoutTypeOrInitializer { get; } =
        new()
        {
            Id = "PRISM1003",
            Title = "Cyclic symbol definition",
            MessageFormat =
                "The type of variable '{0}' cannot be resolved because it lacks both a type annotation and an initializer",
            Severity = DiagnosticSeverity.Error,
        };

    public static DiagnosticDescriptor NumberTooLarge { get; } =
        new()
        {
            Id = "PRISM1004",
            Title = "Numeric literal too larget",
            MessageFormat = "The numeric literal is too large to be represented by the target type",
            Severity = DiagnosticSeverity.Error,
        };

    public static DiagnosticDescriptor LiteralExceedsMaximumMagnitude { get; } =
        new()
        {
            Id = "PRISM1005",
            Title = "Numeric literal too large",
            MessageFormat =
                "The numeric literal cannot be represented because it exceeds the maximum integer literal magnitude",
            Severity = DiagnosticSeverity.Error,
        };

    public static DiagnosticDescriptor UnaryOperatorUndefined { get; } =
        new()
        {
            Id = "PRISM1006",
            Title = "No usable unary operator",
            MessageFormat = "Type '{0}' cannot be used with the unary operator",
            Severity = DiagnosticSeverity.Error,
        };

    public static DiagnosticDescriptor BinaryOperatorUndefined { get; } =
        new()
        {
            Id = "PRISM1007",
            Title = "No usable binary operator",
            MessageFormat = "Types '{0}' and '{1}' cannot be used with the binary operator",
            Severity = DiagnosticSeverity.Error,
        };
}
