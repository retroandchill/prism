// @file SyntaxDiagnosticInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Text;

namespace Prism.Core.Diagnostics;

internal readonly record struct SyntaxDiagnosticInfo(TextSpan RelativeSpan, DiagnosticInfo Info)
{
    public int Offset => RelativeSpan.Start;

    public int Width => RelativeSpan.Length;
}
