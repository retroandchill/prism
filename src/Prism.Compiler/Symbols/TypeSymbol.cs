using Prism.Compiler.Semantic;
using Prism.Metadata;

namespace Prism.Compiler.Symbols;

public readonly record struct SizeAndAlignment(ulong Size, ulong Alignment)
{
    public bool IsZero => Size == 0 && Alignment == 0;
}

public closed class TypeSymbol : MemberContainerSymbol
{
    public virtual SpecialType SpecialType => SpecialType.None;

    public bool IsErrorType { get; private protected init; }

    public bool IsVoid => SpecialType == SpecialType.Void;

    public abstract bool IsDynamicallySized { get; }

    public abstract bool IsDefaultConstructible { get; }

    public sealed override bool Equals(Symbol other, TypeCompareKind kind)
    {
        return other is TypeSymbol typeSymbol && Equals(typeSymbol, kind);
    }

    public virtual bool Equals(TypeSymbol other, TypeCompareKind kind)
    {
        return ReferenceEquals(this, other);
    }
}
