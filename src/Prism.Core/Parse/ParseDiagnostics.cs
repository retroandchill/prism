// @file ParseDiagnostics.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Parse;

public static partial class ParseDiagnostics
{
    [Diagnostic(
        Id = "PRISM0001",
        Severity = DiagnosticSeverity.Error,
        Title = "Unexpected token",
        MessageFormat = "Unexpected token '{0}'",
        Description = "The parser encountered an unexpected token."
    )]
    public static partial Diagnostic UnexpectedToken(Location location, SyntaxKind syntaxKind);

    [Diagnostic(
        Id = "PRISM0002",
        Severity = DiagnosticSeverity.Warning,
        Title = "Empty statement",
        MessageFormat = "Empty statement",
        Description = "The parser encountered an unexpected token."
    )]
    public static partial Diagnostic EmptyStatement(Location location);

    [Diagnostic(
        Id = "PRISM0003",
        Severity = DiagnosticSeverity.Error,
        Title = "Unexpected end of file",
        MessageFormat = "Unexpected end of file"
    )]
    public static partial Diagnostic UnexpectedEndOfFile(Location location);

    [Diagnostic(
        Id = "PRISM0004",
        Severity = DiagnosticSeverity.Warning,
        Title = "Unexpected escape sequence",
        MessageFormat = "Unexpected escape sequence: '{0}'"
    )]
    internal static partial DiagnosticInfo UnexpectedEscape(string sequence);

    [Diagnostic(
        Id = "PRISM0005",
        Severity = DiagnosticSeverity.Error,
        Title = "Unterminated string literal",
        MessageFormat = "Unterminated string literal"
    )]
    internal static partial DiagnosticInfo UnterminatedStringLiteral();

    [Diagnostic(
        Id = "PRISM0006",
        Severity = DiagnosticSeverity.Error,
        Title = "Unterminated block comment",
        MessageFormat = "Unterminated block comment"
    )]
    internal static partial DiagnosticInfo UnterminatedBlockComment();
}
