namespace Prism.Core.Symbols;

public abstract class TypeSymbol : MemberContainerSymbol
{
    private protected TypeSymbol(
        string name,
        Symbol? containingSymbol = null,
        SpecialType specialType = SpecialType.None
    )
        : base(name, containingSymbol)
    {
        SpecialType = specialType;
    }

    public SpecialType SpecialType { get; }

    public bool IsVoid => SpecialType == SpecialType.Void;
}
