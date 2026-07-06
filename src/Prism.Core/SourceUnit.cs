// @file SourceUnit.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Ast;
using Prism.Core.Parse;

namespace Prism.Core;

public sealed class SourceDocument(string sourceText)
{
    public SourceFile SourceFile { get; } = new(sourceText);

    public DiagnosticBag Diagnostics { get; } = new();
}

public sealed class SourceUnit
{
    public SourceFile SourceFile { get; }

    public CompilationUnitSyntax Syntax { get; }

    public DiagnosticBag Diagnostics { get; }

    internal SourceUnit(
        SourceFile sourceFile,
        CompilationUnitSyntax syntax,
        DiagnosticBag diagnostics
    )
    {
        SourceFile = sourceFile;
        Syntax = syntax;
        Diagnostics = diagnostics;
    }
}
