// @file NoOpDiagnosticSink.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Diagnostics;

internal sealed class NullDiagnosticSink : IDiagnosticSink
{
    public static NullDiagnosticSink Instance { get; } = new();

    public void Report(Diagnostic diagnostic)
    {
        // Do nothing
    }
}
