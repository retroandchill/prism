using Prism.Core.Declarations;
using Prism.Core.Diagnostics;

namespace Prism.Core.Binding;

internal sealed class LookupContext
{
    private readonly DiagnosticBag? _diagnostics;

    private LookupContext(DiagnosticBag? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    private static readonly LookupContext Empty = new(null);

    public static LookupContext Create(DiagnosticBag? diagnostics = null)
    {
        return diagnostics is not null ? new LookupContext(diagnostics) : Empty;
    }

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        _diagnostics?.Add(diagnostic);
    }
}
