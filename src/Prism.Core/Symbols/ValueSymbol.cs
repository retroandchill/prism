namespace Prism.Core.Symbols;

public abstract class ValueSymbol : Symbol
{
    private protected ValueSymbol(string name, Symbol? containingSymbol = null)
        : base(name, containingSymbol) { }

    public abstract TypeSymbol Type { get; }

    public abstract bool IsMutable { get; }
}
