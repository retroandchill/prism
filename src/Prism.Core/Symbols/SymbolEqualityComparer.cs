namespace Prism.Core.Symbols;

public sealed class SymbolEqualityComparer : IEqualityComparer<Symbol>
{
    private readonly TypeCompareKind _kind;

    public bool Equals(Symbol? x, Symbol? y)
    {
        return x == y;
    }

    public int GetHashCode(Symbol obj)
    {
        return obj.GetHashCode();
    }
}
