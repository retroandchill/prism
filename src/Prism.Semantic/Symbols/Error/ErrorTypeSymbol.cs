using Prism.Semantic.Symbols.Data;

namespace Prism.Semantic.Symbols.Error;

internal sealed class ErrorTypeSymbol : INamedTypeSymbolData
{
    internal ErrorTypeSymbol(
        string name,
        Symbol? containingSymbol = null,
        SpecialType specialType = SpecialType.None
    )
    {
        Name = name;
        ContainingSymbol = containingSymbol;
        PublicSymbol = new NamedTypeSymbol(this);
    }

    private ErrorTypeSymbol()
        : this("<Error>") { }

    public NamedTypeSymbol PublicSymbol { get; }
    public string Name { get; }
    public Symbol? ContainingSymbol { get; }

    public NamedTypeKind Kind => NamedTypeKind.Class;

    public static readonly ErrorTypeSymbol Unnamed = new();

    public bool IsErrorType => true;

    public bool IsDynamicallySized => false;
    public bool IsDefaultConstructible => false;
}
