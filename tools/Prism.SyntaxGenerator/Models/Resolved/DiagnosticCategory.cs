// @file DiagnosticCategory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class DiagnosticCategory
{
    public string Name { get; }

    public int Start { get; }
    public int End { get; }

    private readonly List<Diagnostic> _diagnostics = [];
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    internal DiagnosticCategory(string name, int start, int end)
    {
        Name = name;
        Start = start;
        End = end;
    }

    internal void EnsureCapacity(int capacity)
    {
        _diagnostics.EnsureCapacity(capacity);
    }

    internal void AddDiagnostic(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }
}
