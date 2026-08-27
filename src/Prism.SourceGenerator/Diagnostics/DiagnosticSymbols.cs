// @file DiagnosticSymbols.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SourceGenerator.Diagnostics;

public static class DiagnosticSymbols
{
    public const string PrismCoreAssembly = "Prism.Core";
    private const string DiagnosticsNamespace = "Prism.Core.Diagnostics";
    public const string Diagnostic = $"{DiagnosticsNamespace}.Diagnostic";
    public const string DiagnosticInfo = $"{DiagnosticsNamespace}.DiagnosticInfo";
    public const string Location = $"{PrismCoreAssembly}.Location";
}
