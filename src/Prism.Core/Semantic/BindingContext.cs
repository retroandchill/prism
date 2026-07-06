// @file BindingContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Prism.Core.Parse;

namespace Prism.Core.Semantic;

public sealed class BindingContext(CompilationContext compilationContext)
{
    private readonly HashSet<SymbolDeclaration> _inProgress =
    [
        with(ReferenceEqualityComparer.Instance),
    ];

    private readonly List<SymbolDeclaration> _stack = [];

    public TargetPlatform Platform => compilationContext.Platform;

    public ReadOnlySpan<SymbolDeclaration> CollectCycle()
    {
        var fullSpan = CollectionsMarshal.AsSpan(_stack);
        var back = fullSpan[^1];
        for (var i = fullSpan.Length - 1; i >= 0; i--)
        {
            if (ReferenceEquals(fullSpan[i], back))
            {
                return fullSpan[i..];
            }
        }

        return default;
    }

    public ReadOnlySpan<char> GetSpan(SourceRange range) => compilationContext.GetSpan(range);

    public bool EnterInference(SymbolDeclaration decl)
    {
        _stack.Add(decl);
        return _inProgress.Add(decl);
    }

    public void ExitInference()
    {
        var decl = _stack[^1];
        _stack.RemoveAt(_stack.Count - 1);
        _inProgress.Remove(decl);
    }

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        compilationContext.ReportDiagnostic(diagnostic);
    }
}
