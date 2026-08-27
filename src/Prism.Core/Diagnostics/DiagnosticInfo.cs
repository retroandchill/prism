using System.Collections.Immutable;

namespace Prism.Core.Diagnostics;

internal sealed class DiagnosticInfo(
    DiagnosticDescriptor descriptor,
    params ImmutableArray<object?> arguments
)
{
    public DiagnosticDescriptor Descriptor { get; } = descriptor;
    public ImmutableArray<object?> Arguments { get; } = arguments;
}
