using System.Collections.Immutable;

namespace Prism.SourceGenerator.Models.Resolved;

public sealed class DiagnosticModel
{
    public ImmutableArray<DiagnosticCategory> Diagnostics { get; }

    internal DiagnosticModel(ImmutableArray<DiagnosticCategory> diagnostics)
    {
        Diagnostics = diagnostics;
    }
}
