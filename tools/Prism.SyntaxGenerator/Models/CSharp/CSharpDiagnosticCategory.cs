// @file CSharpDiagnosticCategory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.SyntaxGenerator.Models.CSharp;

public sealed class CSharpDiagnosticCategory
{
    public string Name { get; }

    public string CSharpName { get; }

    public string DisplayName { get; }

    public int Start { get; }
    public int End { get; }

    public ImmutableArray<CSharpDiagnostic> Diagnostics { get; internal set; } = [];

    internal CSharpDiagnosticCategory(
        string name,
        string cSharpName,
        string displayName,
        int start,
        int end
    )
    {
        Name = name;
        CSharpName = cSharpName;
        DisplayName = displayName;
        Start = start;
        End = end;
    }
}
