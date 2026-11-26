using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;

namespace Prism.Compiler.Symbols.Synthesized;

internal sealed class SynthesizedEnumUnderlyingType
    : SynthesizedSymbol,
        ITypeAliasSymbolData,
        IMemberContainerSymbolData
{
    public SynthesizedEnumUnderlyingType(
        Compilation declaringCompilation,
        NamedTypeSymbol containingType
    )
        : base(declaringCompilation)
    {
        ContainingType = containingType;
        PublicSymbol = new TypeAliasSymbol(this);
    }

    public new TypeAliasSymbol PublicSymbol
    {
        get => Unsafe.As<TypeAliasSymbol>(base.PublicSymbol);
        private init => base.PublicSymbol = value;
    }

    public override string Name => "UnderlyingType";
    public override Symbol ContainingSymbol => ContainingType;

    public override MemberContainerSymbol ContainingMemberContainer => ContainingType;
    public override NamedTypeSymbol ContainingType { get; }

    public bool IsDynamicallySized => AliasedType.IsDynamicallySized;
    public bool IsDefaultConstructible => AliasedType.IsDefaultConstructible;

    public TypeSymbol AliasedType
    {
        get
        {
            var underlyingType = ContainingType.EnumUnderlyingType;
            Debug.Assert(underlyingType is not null);
            return underlyingType;
        }
    }

    public ImmutableArray<Symbol> GetMembers() => AliasedType.GetMembers();

    public ImmutableArray<Symbol> GetMembers(ReadOnlySpan<char> name) =>
        AliasedType.GetMembers(name);
}
