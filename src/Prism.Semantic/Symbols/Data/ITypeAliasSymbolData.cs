namespace Prism.Semantic.Symbols.Data;

internal interface ITypeAliasSymbolData : ITypeSymbolData
{
    public TypeSymbol AliasedType { get; }
}
