using System.Diagnostics.CodeAnalysis;
using Prism.Core.Semantic;

namespace Prism.Core.Symbols;

public enum VariableKind : byte
{
    Global,
    Class,
    Instance,
    Local,
}

public closed class VariableSymbol : ValueSymbol
{
    private protected VariableSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract VariableKind Kind { get; }

    public bool IsStaticStorage => Kind is VariableKind.Global or VariableKind.Class;

    public bool IsInstance => Kind is VariableKind.Instance;

    public bool IsLocal => Kind == VariableKind.Local;

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
