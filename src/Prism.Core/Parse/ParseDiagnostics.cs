// @file ParseDiagnostics.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;

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
    public static partial void UnexpectedToken(this IDiagnosticSink sink, SourceRange range, TokenKind tokenKind);

    [Diagnostic(
        Id = "PRISM0002",
        Severity = DiagnosticSeverity.Warning,
        Title = "Empty statement",
        MessageFormat = "Empty statement",
        Description = "The parser encountered an unexpected token."
    )]
    public static partial void EmptyStatement(this IDiagnosticSink sink, SourceRange range);

    [Diagnostic(
        Id = "PRISM0003",
        Severity = DiagnosticSeverity.Error,
        Title = "Unexpected end of file",
        MessageFormat = "Unexpected end of file"
    )]
    public static partial void UnexpectedEndOfFile(this IDiagnosticSink sink, SourceRange range);

    [Diagnostic(
        Id = "PRISM0004",
        Severity = DiagnosticSeverity.Warning,
        Title = "Unexpected escape sequence",
        MessageFormat = "Unexpected escape sequence: '{0}'"
    )]
    public static partial void UnexpectedEscape(this IDiagnosticSink sink, SourceRange range, ReadOnlyMemory<char> sequence);
}
