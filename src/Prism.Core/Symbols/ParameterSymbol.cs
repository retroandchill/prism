namespace Prism.Core.Symbols;

public abstract class ParameterSymbol : ValueSymbol
{
    private protected ParameterSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract FunctionSymbol ContainingFunction { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
        writer.Write(": ");
        Type.WriteDisplayString(writer);
    }
}
