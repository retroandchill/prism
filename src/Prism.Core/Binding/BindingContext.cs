using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;

namespace Prism.Core.Binding;

[MustDisposeResource]
internal sealed class BindingContext : IDisposable
{
    private static readonly ObjectPool<BindingContext> ContextsWithDiagnostics;
    private static readonly BindingContext Discarded = new(null, null);

    private readonly ObjectPool<BindingContext>? _pool;
    private readonly DiagnosticBag? _diagnostics;

    private BindingContext(ObjectPool<BindingContext>? pool, DiagnosticBag? diagnostics)
    {
        _pool = pool;
        _diagnostics = diagnostics;
    }

    [MustDisposeResource]
    public static BindingContext Create(bool withDiagnostics = true)
    {
        return withDiagnostics ? ContextsWithDiagnostics.Get() : Discarded;
    }

    public IEnumerable<Diagnostic> AccumulatedDiagnostics =>
        (IEnumerable<Diagnostic>?)_diagnostics ?? [];

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        _diagnostics?.Add(diagnostic);
    }

    public void ReportDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        _diagnostics?.AddRange(diagnostics);
    }

    public void ReportDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        _diagnostics?.AddRange(diagnostics);
    }

    public void Dispose()
    {
        _pool?.Return(this);
    }

    private sealed class ContextObjectPolicy(Func<BindingContext> factory)
        : IPooledObjectPolicy<BindingContext>
    {
        public BindingContext Create()
        {
            return factory();
        }

        public bool Return(BindingContext obj)
        {
            throw new NotImplementedException();
        }
    }

    static BindingContext()
    {
        var provider = new DefaultObjectPoolProvider();
        ContextsWithDiagnostics = provider.Create(
            new ContextObjectPolicy(() => new BindingContext(ContextsWithDiagnostics, []))
        );
    }
}
