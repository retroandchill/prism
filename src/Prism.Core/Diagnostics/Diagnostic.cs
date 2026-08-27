// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Prism.Core.Parse;

namespace Prism.Core.Diagnostics;

public sealed class Diagnostic
{
    internal DiagnosticInfo Info { get; }
    public DiagnosticDescriptor Descriptor => Info.Descriptor;
    public Location Location { get; }

    internal Diagnostic(DiagnosticInfo info, Location location)
    {
        Info = info;
        Location = location;
    }

    public static Diagnostic Create(
        DiagnosticDescriptor descriptor,
        Location location,
        params ImmutableArray<object?> arguments
    )
    {
        return new Diagnostic(new DiagnosticInfo(descriptor, arguments), location);
    }

    public string GetMessage(IFormatProvider? provider = null)
    {
        return string.Format(
            provider,
            Descriptor.CompositeFormat,
            ImmutableCollectionsMarshal.AsArray(Info.Arguments) ?? []
        );
    }

    public override string ToString()
    {
        return $"{Descriptor.Id}: {GetMessage()}";
    }
}
