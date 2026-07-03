// @file ParseDiagnostics.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Parse;

public static class ParseDiagnostics
{
    public static readonly DiagnosticDescriptor UnexpectedToken = new()
    {
        Id = "PRISM0001",
        Severity = DiagnosticSeverity.Error,
        Title = "Unexpected token",
        MessageFormat = "Unexpected token '{0}'",
        Description = "The parser encountered an unexpected token.",
    };

    public static readonly DiagnosticDescriptor EmptyStatement = new()
    {
        Id = "PRISM0002",
        Severity = DiagnosticSeverity.Warning,
        Title = "Empty statement",
        MessageFormat = "Empty statement",
        Description = "The parser encountered an empty statement.",
    };

    public static readonly DiagnosticDescriptor UnexpectedEndOfFile = new()
    {
        Id = "PRISM0003",
        Severity = DiagnosticSeverity.Warning,
        Title = "Unexpected end of file",
        MessageFormat = "Unexpected end of file",
        Description = "The parser encountered an unexpected end of a file",
    };
}
