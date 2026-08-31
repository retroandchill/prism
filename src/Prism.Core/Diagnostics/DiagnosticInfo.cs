// @file DiagnosticInfo.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Prism.Core.Diagnostics;

internal sealed class DiagnosticInfo(
    DiagnosticDescriptor descriptor,
    DiagnosticSeverity severity,
    params ImmutableArray<object?> arguments
)
{
    public DiagnosticInfo(DiagnosticDescriptor descriptor, params ImmutableArray<object?> arguments)
        : this(descriptor, descriptor.DefaultSeverity, arguments) { }

    public DiagnosticDescriptor Descriptor { get; } = descriptor;

    public string Id => Descriptor.Id;

    public string Title => Descriptor.Title;

    public string Category => Descriptor.Category;

    public DiagnosticSeverity Severity { get; } = severity;

    public DiagnosticSeverity DefaultSeverity => Descriptor.DefaultSeverity;

    public bool IsEnabledByDefault => Descriptor.IsEnabledByDefault;

    public bool IsWarningAsError =>
        DefaultSeverity == DiagnosticSeverity.Warning && Severity == DiagnosticSeverity.Error;

    public ImmutableArray<object?> Arguments { get; } = arguments;

    public ImmutableArray<string> CustomTags => Descriptor.Tags;

    public string GetMessage()
    {
        if (Arguments.IsDefaultOrEmpty)
            return Descriptor.MessageFormat;

        // This is safe because string.Format doesn't modify the arguments arrayu
        return string.Format(
            CultureInfo.CurrentCulture,
            Descriptor.MessageFormat,
            ImmutableCollectionsMarshal.AsArray(Arguments) ?? []
        );
    }
}
