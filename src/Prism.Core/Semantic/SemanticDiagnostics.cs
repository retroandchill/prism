// @file SemanticDiagnostics.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;
using Prism.Core.Parse;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public static partial class SemanticDiagnostics
{
    [Diagnostic(
        Id = "PRISM1001",
        Title = "Duplicate symbol definition",
        MessageFormat = "Symbol '{0}' is already defined in this scope.",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void DuplicateSymbolDefinition(this IDiagnosticSink sink, SourceRange range, Name name);

    [Diagnostic(
        Id = "PRISM1002",
        Title = "Cyclic symbol definition",
        MessageFormat = "Symbol '{0}' cannot be resolved due to a cyclic reference:\n{1}",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void CyclicSymbolDefinition(this IDiagnosticSink sink, SourceRange range, Name source,
        string chain);

    [Diagnostic(
        Id = "PRISM1003",
        Title = "Cyclic symbol definition",
        MessageFormat =
            "The type of variable '{0}' cannot be resolved because it lacks both a type annotation and an initializer",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void
        VariableWithoutTypeOrInitializer(this IDiagnosticSink sink, SourceRange range, Name name);

    [Diagnostic(
        Id = "PRISM1004",
        Title = "Numeric literal too larget",
        MessageFormat = "The numeric literal is too large to be represented by the target type",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void NumberTooLarge(this IDiagnosticSink sink, SourceRange range);

    [Diagnostic(
        Id = "PRISM1006",
        Title = "No usable unary operator",
        MessageFormat = "Type '{0}' cannot be used with the unary operator",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void UnaryOperatorUndefined(this IDiagnosticSink sink, SourceRange range, Name name);

    [Diagnostic(
        Id = "PRISM1007",
        Title = "No usable binary operator",
        MessageFormat = "Types '{0}' and '{1}' cannot be used with the binary operator",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void BinaryOperatorUndefined(this IDiagnosticSink sink, SourceRange range, Name left,
        Name right);
    
    [Diagnostic(
        Id = "PRISM1008",
        Title = "No implicit conversion",
        MessageFormat = "No implicit conversion from '{0}' to '{1}'",
        Severity = DiagnosticSeverity.Error
    )]
    public static partial void NoImplicitConversion(this IDiagnosticSink sink, SourceRange range,
        Name from, Name to);
}
