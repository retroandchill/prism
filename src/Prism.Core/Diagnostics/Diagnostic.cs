// @file Diagnostic.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Diagnostics;

public sealed class Diagnostic
{
    internal Diagnostic(DiagnosticInfo info, Location location, bool isSuppressed = false)
    {
        Info = info;
        Location = location;
        IsSuppressed = isSuppressed;
    }

    internal DiagnosticInfo Info { get; }

    public Location Location { get; }

    public bool IsSuppressed { get; }

    public DiagnosticDescriptor Descriptor => Info.Descriptor;

    public string Id => Descriptor.Id;

    public string Category => Descriptor.Category;

    public DiagnosticSeverity Severity => Info.Severity;

    public string GetMessage() => Info.GetMessage();
}
