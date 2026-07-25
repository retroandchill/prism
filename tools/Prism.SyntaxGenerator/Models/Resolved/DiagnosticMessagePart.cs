// @file SyntaxDiagnosticMessagePart.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Resolved;

public abstract class DiagnosticMessagePart;

public sealed class DiagnosticMessageTextPart : DiagnosticMessagePart
{
    public string Text { get; }

    internal DiagnosticMessageTextPart(string text)
    {
        Text = text;
    }
}

public sealed class DiagnosticMessageArgumentPart : DiagnosticMessagePart
{
    public DiagnosticArgument Argument { get; }

    internal DiagnosticMessageArgumentPart(DiagnosticArgument argument)
    {
        Argument = argument;
    }
}
