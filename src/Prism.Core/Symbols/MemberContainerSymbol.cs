using System.Collections.Immutable;

namespace Prism.Core.Symbols;

public abstract class MemberContainerSymbol : Symbol
{
    private protected MemberContainerSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public abstract ImmutableArray<Symbol> GetMembers();

    public abstract ImmutableArray<Symbol> GetMembers(string name);
}
