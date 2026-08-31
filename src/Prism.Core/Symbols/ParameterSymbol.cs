namespace Prism.Core.Symbols;

public abstract class ParameterSymbol : Symbol
{
    private protected ParameterSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract TypeSymbol Type { get; }

    public abstract bool IsMutable { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
        writer.Write(": ");
        Type.WriteDisplayString(writer);
    }
}
