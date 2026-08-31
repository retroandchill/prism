// @file SyntaxDiagnosticArgument.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class DiagnosticArgument
{
    public string Name { get; }

    public string Type { get; }

    public Diagnostic Owner { get; }

    internal DiagnosticArgument(string name, string type, Diagnostic owner)
    {
        Name = name;
        Type = type;
        Owner = owner;
    }
}
