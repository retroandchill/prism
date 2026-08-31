// @file CSharpDiagnosticMessagePart.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.CSharp;

public abstract class CSharpDiagnosticMessagePart;

public sealed class CSharpDiagnosticMessageTextPart : CSharpDiagnosticMessagePart
{
    public string Text { get; }

    internal CSharpDiagnosticMessageTextPart(string text)
    {
        Text = text;
    }
}

public sealed class CSharpDiagnosticMessageArgumentPart : CSharpDiagnosticMessagePart
{
    public CSharpDiagnosticArgument Argument { get; }

    internal CSharpDiagnosticMessageArgumentPart(CSharpDiagnosticArgument argument)
    {
        Argument = argument;
    }
}
