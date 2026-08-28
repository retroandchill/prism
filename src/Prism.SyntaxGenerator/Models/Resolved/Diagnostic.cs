// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Resolved;

public sealed class Diagnostic
{
    public string Name { get; }

    public int Value { get; }

    public DiagnosticCategory Category { get; }

    public DiagnosticSeverity Severity { get; }

    public string Format { get; }

    public string Explanation { get; }

    public string HelpLink { get; }

    private readonly List<DiagnosticArgument> _arguments = [];
    public IReadOnlyList<DiagnosticArgument> Arguments => _arguments;

    private readonly List<DiagnosticMessagePart> _messageParts = [];
    public IReadOnlyList<DiagnosticMessagePart> MessageParts => _messageParts;

    public ImmutableArray<string> Tags { get; }

    internal Diagnostic(
        string name,
        int value,
        DiagnosticCategory category,
        DiagnosticSeverity severity,
        string format,
        string explanation,
        string helpLink,
        ImmutableArray<string> tags
    )
    {
        Name = name;
        Value = value;
        Category = category;
        Severity = severity;
        Format = format;
        Explanation = explanation;
        HelpLink = helpLink;
        Tags = tags;
    }

    internal void EnsureCapacity(int capacity)
    {
        _arguments.EnsureCapacity(capacity);
    }

    internal void AddArgument(DiagnosticArgument argument)
    {
        _arguments.Add(argument);
    }

    internal void AddMessagePart(DiagnosticMessagePart messagePart)
    {
        _messageParts.Add(messagePart);
    }
}
