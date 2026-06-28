// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.Source;

namespace Prism.Core;

public enum Severity
{
    Error,
    Warning,
    Info,
    Hint,
}

public sealed record Diagnostic(
    DiagnosticDescriptor Descriptor,
    TextSpan Span,
    params ImmutableArray<object?> Arguments
);

public sealed record DiagnosticDescriptor(
    string Id,
    string Title,
    Severity Severity,
    FormattableString MessageFormat
);

public sealed class DiagnosticSink
{
    private readonly List<Diagnostic> _diagnostics = [];

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public void Report(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }
}
