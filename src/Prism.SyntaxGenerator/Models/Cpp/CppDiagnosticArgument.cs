// @file CppDiagnosticArgument.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppDiagnosticArgument
{
    public string Name { get; }

    public string CppName { get; }

    public CppDiagnostic Owner { get; internal set; } = null!;

    public string Type { get; }

    public string CppType { get; }

    public bool NeedsMove { get; }

    internal CppDiagnosticArgument(
        string name,
        string cppName,
        string type,
        string cppType,
        bool needsMove
    )
    {
        Name = name;
        CppName = cppName;
        Type = type;
        CppType = cppType;
        NeedsMove = needsMove;
    }
}
