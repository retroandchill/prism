using System.Runtime.CompilerServices;
using Prism.Core.Compiling;

namespace Prism.Core.Symbols;

internal sealed class SymbolLocationComparer : IComparer<Symbol>
{
    private readonly Compilation _compilation;
    private static readonly ConditionalWeakTable<Compilation, SymbolLocationComparer> Comparers =
        new();

    private SymbolLocationComparer(Compilation compilation)
    {
        _compilation = compilation;
    }

    public static SymbolLocationComparer Get(Compilation compilation)
    {
        return Comparers.GetOrAdd(compilation, static c => new SymbolLocationComparer(c));
    }

    public int Compare(Symbol? x, Symbol? y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        if (x is null)
            return -1;
        if (y is null)
            return 1;

        return _compilation.CompareSourceLocations(x.FirstLocation(), y.FirstLocation());
    }
}
