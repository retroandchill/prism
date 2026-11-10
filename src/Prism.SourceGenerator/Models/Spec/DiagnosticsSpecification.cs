using System.Collections.Immutable;

namespace Prism.SourceGenerator.Models.Spec;

public sealed record DiagnosticsSpecification
{
    public required int Version { get; init; }
    public required ImmutableArray<DiagnosticCategoryDefinition> Diagnostics { get; init; }
}
