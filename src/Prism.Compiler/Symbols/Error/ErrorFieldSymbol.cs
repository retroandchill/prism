using Prism.Constants;
using Prism.Semantic.Symbols;
using Prism.Semantic.Symbols.Data;

namespace Prism.Compiler.Symbols.Error;

public class ErrorFieldSymbol : IVariableSymbolData
{
    internal ErrorFieldSymbol(string name, TypeSymbol containingSymbol)
    {
        Name = name;
        ContainingType = containingSymbol;
        PublicSymbol = new VariableSymbol(this);
    }

    public VariableSymbol PublicSymbol { get; }
    public string Name { get; }
    public Symbol ContainingSymbol => ContainingType;
    public MemberContainerSymbol? ContainingMemberContainer => ContainingType;
    public TypeSymbol ContainingType { get; }
    public TypeSymbol Type => ErrorTypeSymbol.Unnamed.PublicSymbol;
    public bool IsMutable => false;
    public VariableKind Kind => VariableKind.Instance;
    public bool HasInitializer => false;
    public ConstantValue? ConstantValue => null;
}
