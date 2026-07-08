// @file CompilationContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;
using Prism.Core.Parse;

namespace Prism.Core;

public class CompilationContext(
    SourceFile sourceFile,
    CancellationToken cancellationToken = default
)
{
    private readonly List<Diagnostic> _diagnostics = [];

    public SourceFile SourceFile { get; init; } = sourceFile;
    public CancellationToken CancellationToken { get; } = cancellationToken;
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    public bool HasErrors { get; private set; }

    public TargetPlatform Platform { get; init; } = TargetPlatform.Default;

    public ReadOnlySpan<char> GetSpan(TextSpan range) => SourceFile.GetSpan(range);

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        _diagnostics.Add(diagnostic);
        if (diagnostic.Descriptor.Severity == DiagnosticSeverity.Error)
        {
            HasErrors = true;
        }
    }
}
