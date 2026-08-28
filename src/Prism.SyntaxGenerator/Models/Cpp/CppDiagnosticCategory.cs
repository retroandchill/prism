// @file CppDiagnosticCategory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppDiagnosticCategory
{
    public string Name { get; }

    public string CppName { get; }

    public string DisplayName { get; }

    public int Start { get; }
    public int End { get; }

    public ImmutableArray<CppDiagnostic> Diagnostics { get; internal set; } = [];

    internal CppDiagnosticCategory(
        string name,
        string cppName,
        string displayName,
        int start,
        int end
    )
    {
        Name = name;
        CppName = cppName;
        DisplayName = displayName;
        Start = start;
        End = end;
    }
}
