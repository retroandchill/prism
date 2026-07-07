// @file DiagnosticBag.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;

namespace Prism.Core;

public sealed class DiagnosticBag :  IDiagnosticSink, IReadOnlyList<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];
    public bool HasErrors { get; private set; }

    public int Count => _diagnostics.Count;

    public Diagnostic this[int index] => _diagnostics[index];

    public void Report(Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        _diagnostics.Add(diagnostic);
        if (diagnostic.Descriptor.Severity == DiagnosticSeverity.Error)
        {
            HasErrors = true;
        }
    }

    public IEnumerator<Diagnostic> GetEnumerator()
    {
        return _diagnostics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
