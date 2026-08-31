// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;

namespace Prism.Core.Diagnostics;

public sealed class Diagnostic
{
    internal Diagnostic(DiagnosticInfo info, Location location)
    {
        Info = info;
        Location = location;
    }

    private Diagnostic(
        DiagnosticInfo info,
        Location location,
        ImmutableArray<Location> additionalLocations
    )
    {
        Info = info;
        Location = location;
        AdditionalLocations = additionalLocations;
    }

    public Diagnostic(
        DiagnosticDescriptor descriptor,
        Location location,
        params ImmutableArray<object?> args
    )
        : this(new DiagnosticInfo(descriptor, args), location) { }

    public Diagnostic(
        DiagnosticDescriptor descriptor,
        DiagnosticSeverity severity,
        Location location,
        params ImmutableArray<object?> args
    )
        : this(new DiagnosticInfo(descriptor, severity, args), location) { }

    public Diagnostic(
        DiagnosticDescriptor descriptor,
        Location location,
        ImmutableArray<Location> additionalLocations,
        params ImmutableArray<object?> args
    )
        : this(new DiagnosticInfo(descriptor, args), location, additionalLocations) { }

    public Diagnostic(
        DiagnosticDescriptor descriptor,
        DiagnosticSeverity severity,
        Location location,
        ImmutableArray<Location> additionalLocations,
        params ImmutableArray<object?> args
    )
        : this(new DiagnosticInfo(descriptor, severity, args), location, additionalLocations) { }

    internal DiagnosticInfo Info { get; }

    public Location Location { get; }

    public ImmutableArray<Location> AdditionalLocations { get; } = [];

    public DiagnosticDescriptor Descriptor => Info.Descriptor;

    public string Id => Descriptor.Id;

    public string Category => Descriptor.Category;

    public DiagnosticSeverity Severity => Info.Severity;

    public string GetMessage() => Info.GetMessage();
}
