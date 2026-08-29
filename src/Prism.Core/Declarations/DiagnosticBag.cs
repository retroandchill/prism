using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Diagnostics;

namespace Prism.Core.Declarations;

internal sealed class DiagnosticBag
{
    private ConcurrentQueue<Diagnostic>? _diagnostics;

    private ConcurrentQueue<Diagnostic> Diagnostics
    {
        get
        {
            if (_diagnostics is not null)
                return _diagnostics;

            Interlocked.CompareExchange(ref _diagnostics, new ConcurrentQueue<Diagnostic>(), null);
            return _diagnostics;
        }
    }

    [MemberNotNullWhen(false, nameof(_diagnostics))]
    public bool IsEmpty => _diagnostics is null || _diagnostics.IsEmpty;

    public bool HasErrors
    {
        get { return !IsEmpty && Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error); }
    }

    public void Add(Diagnostic diagnostic)
    {
        Diagnostics.Enqueue(diagnostic);
    }

    public void AddRange(IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            Diagnostics.Enqueue(diagnostic);
    }

    public void AddRange(DiagnosticBag other)
    {
        if (other._diagnostics is null)
            return;

        foreach (var item in other._diagnostics)
            Diagnostics.Enqueue(item);
    }

    public void AddRangeAndClear(DiagnosticBag other)
    {
        AddRange(other);
        other.Clear();
    }

    public ImmutableArray<Diagnostic> ToImmutable()
    {
        return _diagnostics?.ToImmutableArray() ?? [];
    }

    public ImmutableArray<Diagnostic> ToImmutableAndClear()
    {
        var result = ToImmutable();
        Clear();
        return result;
    }

    public void Clear()
    {
        _diagnostics?.Clear();
    }
}
