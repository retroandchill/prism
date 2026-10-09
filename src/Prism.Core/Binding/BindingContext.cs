using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using Prism.Core.Declarations;
using Prism.Core.Diagnostics;
using Prism.Core.Symbols;

namespace Prism.Core.Binding;

[MustDisposeResource]
internal sealed class BindingContext : IDisposable
{
    private static readonly ObjectPool<BindingContext> ContextsWithDiagnostics;
    private static readonly ObjectPool<BindingContext> ContextsWithDependencies;
    private static readonly ObjectPool<BindingContext> ContextsWithBoth;
    public static readonly BindingContext Discarded = new(null, null, null);

    private readonly ObjectPool<BindingContext>? _pool;
    private readonly DiagnosticBag? _diagnostics;
    private readonly HashSet<AssemblySymbol>? _dependencies;

    private BindingContext(
        ObjectPool<BindingContext>? pool,
        DiagnosticBag? diagnostics,
        HashSet<AssemblySymbol>? dependencies
    )
    {
        _pool = pool;
        _diagnostics = diagnostics;
        _dependencies = dependencies;
    }

    [MustDisposeResource]
    public static BindingContext Create(bool withDiagnostics = true, bool withDependencies = true)
    {
        return (withDiagnostics, withDependencies) switch
        {
            (true, true) => ContextsWithBoth.Get(),
            (true, false) => ContextsWithDiagnostics.Get(),
            (false, true) => ContextsWithDependencies.Get(),
            (false, false) => Discarded,
        };
    }

    public IEnumerable<Diagnostic> AccumulatedDiagnostics =>
        (IEnumerable<Diagnostic>?)_diagnostics ?? [];

    public bool HasErrors => _diagnostics?.HasErrors ?? false;

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

    public ImmutableArray<Diagnostic> CollectDiagnostics()
    {
        return _diagnostics?.ToImmutable() ?? [];
    }

    public void AddDependency(AssemblySymbol dependency)
    {
        _dependencies?.Add(dependency);
    }

    public ImmutableHashSet<AssemblySymbol> CollectDependencies()
    {
        return _dependencies?.ToImmutableHashSet() ?? ImmutableHashSet<AssemblySymbol>.Empty;
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
            obj._diagnostics?.Clear();
            obj._dependencies?.Clear();
            return true;
        }
    }

    static BindingContext()
    {
        var provider = new DefaultObjectPoolProvider();
        ContextsWithDiagnostics = provider.Create(
            new ContextObjectPolicy(() => new BindingContext(ContextsWithDiagnostics, [], null))
        );
        ContextsWithDependencies = provider.Create(
            new ContextObjectPolicy(() => new BindingContext(ContextsWithDependencies, null, []))
        );
        ContextsWithBoth = provider.Create(
            new ContextObjectPolicy(() => new BindingContext(ContextsWithBoth, [], []))
        );
    }
}
