using System.Diagnostics.CodeAnalysis;
using Prism.Core.Semantic;

namespace Prism.Core.Symbols;

public closed class VariableSymbol : ValueSymbol
{
    private protected VariableSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract bool IsGlobal { get; }

    public bool IsLocal => !IsGlobal;

    public abstract bool HasInitializer { get; }

    [MemberNotNullWhen(true, nameof(ConstantValue))]
    public bool IsConst => ConstantValue is not null;

    public abstract ConstantValue? ConstantValue { get; }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        writer.Write(Name);
        writer.Write(": ");
        Type.WriteDisplayString(writer);
    }
}
