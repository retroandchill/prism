using System.Collections.Immutable;
using Prism.Compiler.Abi;
using Prism.Diagnostics;
using Prism.Metadata;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Symbols.Error;

internal sealed class ErrorFunctionSymbol : IFunctionSymbolData
{
    private ErrorFunctionSymbol(
        string name,
        Symbol? containingSymbol = null,
        TypeSymbol? returnType = null
    )
    {
        Name = name;
        ContainingSymbol = containingSymbol;
        ReturnType = returnType ?? ErrorTypeSymbol.Unnamed.PublicSymbol;
        PublicSymbol = new FunctionSymbol(this);
    }

    private ErrorFunctionSymbol()
        : this("") { }

    public FunctionSymbol PublicSymbol { get; }

    public static readonly ErrorFunctionSymbol Unnamed = new();

    public string Name { get; }
    public Symbol? ContainingSymbol { get; }
    public TypeSymbol ReturnType { get; }
    public TypeSymbol? ReceiverType => null;
    public ImmutableArray<ParameterSymbol> Parameters => [];
    public FunctionKind Kind => FunctionKind.Ordinary;
    public AbiKind AbiKind => AbiKind.Internal;
    public bool IsExtern => false;
}
