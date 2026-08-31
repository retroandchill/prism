using Prism.Core.Semantic;

namespace Prism.Core.Symbols;

public abstract class VariableSymbol : Symbol
{
    private protected VariableSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract TypeSymbol Type { get; }

    public abstract bool IsMutable { get; }

    public abstract bool IsGlobal { get; }

    public abstract bool HasInitializer { get; }

    public abstract ConstantValue? ConstantValue { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
        writer.Write(": ");
        Type.WriteDisplayString(writer);
    }
}
