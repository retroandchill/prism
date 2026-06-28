// @file Parser.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Source;

namespace Prism.Core.Parse;

public class Parser(SourceFile sourceFile, DiagnosticSink diagnosticSink)
{
    private readonly SourceFile _sourceFile = sourceFile;
    private TokenStream _stream = new(sourceFile.Text);
    private DiagnosticSink _diagnosticSink = diagnosticSink;
}
