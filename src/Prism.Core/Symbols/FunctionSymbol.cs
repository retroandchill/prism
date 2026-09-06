using System.Collections.Immutable;
using ZLinq;

namespace Prism.Core.Symbols;

public abstract class FunctionSymbol : Symbol
{
    private protected FunctionSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract TypeSymbol ReturnType { get; }

    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

    public bool ReturnsVoid => ReturnType.IsVoid;

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        if (ContainingSymbol is not null or AssemblySymbol or NamespaceSymbol { IsGlobal: false })
        {
            ContainingSymbol.WriteDisplayString(writer);
            writer.Write("::");
        }

        writer.Write(Name);
        writer.Write('(');
        foreach (var (i, param) in Parameters.AsValueEnumerable().Index())
        {
            if (i > 0)
                writer.Write(", ");

            param.WriteDisplayString(writer);
        }
        writer.Write(')');
        if (ReturnsVoid)
            return;

        writer.Write(": ");
        ReturnType.WriteDisplayString(writer);
    }
}
