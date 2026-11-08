namespace Prism.Core.Symbols;

public closed class ValueSymbol : Symbol
{
    public abstract TypeSymbol Type { get; }

    public abstract bool IsMutable { get; }
}
