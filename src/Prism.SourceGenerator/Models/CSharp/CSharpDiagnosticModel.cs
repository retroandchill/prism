using System.Collections.Immutable;

namespace Prism.SourceGenerator.Models.CSharp;

public class CSharpDiagnosticModel
{
    public ImmutableArray<CSharpDiagnosticCategory> Diagnostics { get; }

    internal CSharpDiagnosticModel(ImmutableArray<CSharpDiagnosticCategory> diagnostics)
    {
        Diagnostics = diagnostics;
    }
}
