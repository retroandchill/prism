// @file CSharpDiagnosticArgument.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.CSharp;

public sealed class CSharpDiagnosticArgument
{
    public string Name { get; }

    public string CSharpName { get; }

    public CSharpDiagnostic Owner { get; internal set; } = null!;

    public string Type { get; }

    public string CSharpType { get; }

    internal CSharpDiagnosticArgument(
        string name,
        string cSharpName,
        string type,
        string cSharpType
    )
    {
        Name = name;
        CSharpName = cSharpName;
        Type = type;
        CSharpType = cSharpType;
    }
}
