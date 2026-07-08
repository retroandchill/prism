// @file DiagnosticDescriptor.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Prism.Core.Diagnostics;

public sealed record DiagnosticDescriptor
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required DiagnosticSeverity Severity { get; init; }

    [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
    public required string MessageFormat { get; init; }
}
