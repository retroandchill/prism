// @file SyntaxDiagnosticInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Text;

namespace Prism.Core.Diagnostics;

internal readonly record struct SyntaxDiagnosticInfo(
    DiagnosticInfo Info,
    TextSpan RelativeSpan = default
)
{
    public int Offset => RelativeSpan.Start;

    public int Width => RelativeSpan.Length;

    public SyntaxDiagnosticInfo(DiagnosticInfo info, int offset)
        : this(info, new TextSpan(offset, 0)) { }

    public SyntaxDiagnosticInfo(DiagnosticInfo info, int offset, int width)
        : this(info, new TextSpan(offset, width)) { }
}
