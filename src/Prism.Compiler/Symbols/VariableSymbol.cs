using System.Diagnostics.CodeAnalysis;
using Prism.Constants;

namespace Prism.Compiler.Symbols;

public closed class VariableSymbol : ValueSymbol
{
    public abstract VariableKind Kind { get; }

    public virtual FunctionSymbol? ContainingFunction => null;

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
