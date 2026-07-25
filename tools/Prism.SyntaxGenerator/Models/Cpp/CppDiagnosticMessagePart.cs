// @file CppDiagnosticMessagePart.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.SyntaxGenerator.Models.Cpp;

public abstract class CppDiagnosticMessagePart;

public sealed class CppDiagnosticMessageTextPart : CppDiagnosticMessagePart
{
    public string Text { get; }

    internal CppDiagnosticMessageTextPart(string text)
    {
        Text = text;
    }
}

public sealed class CppDiagnosticMessageArgumentPart : CppDiagnosticMessagePart
{
    public CppDiagnosticArgument Argument { get; }

    internal CppDiagnosticMessageArgumentPart(CppDiagnosticArgument argument)
    {
        Argument = argument;
    }
}
