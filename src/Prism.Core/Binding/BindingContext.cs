using System.Collections.Immutable;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;

namespace Prism.Core.Binding;

internal sealed class BindingContext
{
    private readonly DiagnosticBag? _diagnostics;

    private ImmutableHashSet<Symbol>.Builder? _referencedLocals;

    private BindingContext(DiagnosticBag? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    private static readonly BindingContext Empty = new(null);

    public static BindingContext Create(DiagnosticBag? diagnostics = null)
    {
        return diagnostics is not null ? new BindingContext(diagnostics) : Empty;
    }

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        _diagnostics?.Add(diagnostic);
    }

    public void AddReferencedLocal(Symbol symbol)
    {
        if (_referencedLocals is null)
        {
            Interlocked.CompareExchange(
                ref _referencedLocals,
                ImmutableHashSet.CreateBuilder<Symbol>(ReferenceEqualityComparer.Instance),
                null
            );
        }

        _referencedLocals.Add(symbol);
    }

    public ImmutableHashSet<Symbol> CollectReferencedLocals()
    {
        return _referencedLocals is not null
            ? _referencedLocals.ToImmutable()
            : ImmutableHashSet<Symbol>.Empty;
    }
}
