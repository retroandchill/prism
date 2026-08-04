// @file CppDiagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Cpp;

public sealed class CppDiagnostic
{
    public string Name { get; }

    public string CppName { get; }

    public string SymbolName { get; }

    public string Title { get; }

    public int Value { get; }

    public CppDiagnosticCategory Category { get; internal set; } = null!;

    public DiagnosticSeverity Severity { get; }

    public string Format { get; }

    public string Explanation { get; internal set; } = "";

    public string HelpLink { get; internal set; } = "";

    public ImmutableArray<string> Tags { get; internal set; } = [];

    public ImmutableArray<CppDiagnosticArgument> Arguments { get; internal set; } = [];

    public ImmutableArray<CppDiagnosticMessagePart> MessageParts { get; internal set; } = [];

    internal CppDiagnostic(
        string name,
        string cppName,
        string symbolName,
        string title,
        int value,
        DiagnosticSeverity severity,
        string format
    )
    {
        Name = name;
        CppName = cppName;
        SymbolName = symbolName;
        Title = title;
        Value = value;
        Severity = severity;
        Format = format;
    }
}
