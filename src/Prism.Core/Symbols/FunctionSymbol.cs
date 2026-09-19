using System.Collections.Immutable;
using Prism.Core.Abi;
using ZLinq;

namespace Prism.Core.Symbols;

public closed class FunctionSymbol : Symbol
{
    private protected FunctionSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract TypeSymbol ReturnType { get; }

    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

    public bool ReturnsVoid => ReturnType.IsVoid;

    internal abstract AbiKind AbiKind { get; }

    internal ImmutableArray<TypeSymbol> ParameterTypes
    {
        get
        {
            if (!field.IsDefault)
                return field;

            ImmutableInterlocked.InterlockedInitialize(
                ref field,
                [.. Parameters.Select(p => p.Type)]
            );
            return field;
        }
    }

    public abstract bool IsExtern { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        if (
            ContainingSymbol
            is not null
                and not AssemblySymbol
                and NamespaceSymbol { IsGlobal: false }
        )
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
