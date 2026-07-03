// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Prism.Core.Parse;

namespace Prism.Core;

public enum DiagnosticSeverity : byte
{
    Error,
    Warning,
    Info,
    Hint,
}

public sealed record DiagnosticDescriptor
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required DiagnosticSeverity Severity { get; init; }
    public bool EnabledByDefault { get; init; } = true;

    [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
    public required string MessageFormat { get; init; }

    internal CompositeFormat CompiledFormat => field ??= CompositeFormat.Parse(MessageFormat);
}

public sealed record Diagnostic
{
    public required DiagnosticDescriptor Descriptor { get; init; }
    public required SourceRange Range { get; init; }
    public ImmutableArray<object?> Arguments { get; init; } = [];

    public string GetMessage(IFormatProvider? provider = null)
    {
        return string.Format(
            provider ?? CultureInfo.InvariantCulture,
            Descriptor.CompiledFormat,
            ImmutableCollectionsMarshal.AsArray(Arguments) ?? []
        );
    }

    public override string ToString()
    {
        return $"{Descriptor.Id}: {GetMessage()}";
    }
}
