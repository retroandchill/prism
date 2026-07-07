// @file DiagnosticSymbols.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SourceGenerator.Diagnostics;

public static class DiagnosticSymbols
{
    private const string DiagnosticsNamespace = "Prism.Core.Diagnostics";
    private const string ParseNamespace = "Prism.Core.Parse";
    public const string IDiagnosticSink = $"{DiagnosticsNamespace}.IDiagnosticSink";
    public const string SourceRange = $"{ParseNamespace}.SourceRange";
}