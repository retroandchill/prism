using System.Runtime.CompilerServices;
using Cysharp.Text;
using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols;

public sealed class TypeAliasSymbol : TypeSymbol
{
    internal TypeAliasSymbol(ITypeAliasSymbolData data)
        : base(data) { }

    internal new ITypeAliasSymbolData Data => Unsafe.As<ITypeAliasSymbolData>(base.Data);

    public TypeSymbol AliasedType => Data.AliasedType;

    public override void WriteDisplayString(ref Utf16ValueStringBuilder builder)
    {
        if (ContainingNamespace is { IsGlobal: false } container)
        {
            container.WriteDisplayString(ref builder);
            builder.Append("::");
        }

        builder.Append(Name);
    }

    public override bool Equals(TypeSymbol other, SymbolCompareKind kind)
    {
        if (other is TypeAliasSymbol typeAliasSymbol)
            return AliasedType.Equals(typeAliasSymbol.AliasedType, kind);

        return AliasedType.Equals(other, kind);
    }

    public override int GetHashCode(SymbolCompareKind kind)
    {
        return AliasedType.GetHashCode(kind);
    }
}
