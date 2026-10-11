using System.Collections.Immutable;
using Prism.Core.Abi;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorFunctionSymbol : FunctionSymbol
{
    private ErrorFunctionSymbol(
        string name,
        Symbol? containingSymbol = null,
        TypeSymbol? returnType = null
    )
        : base(name, containingSymbol)
    {
        ReturnType = returnType ?? ErrorTypeSymbol.Unnamed;
    }

    private ErrorFunctionSymbol()
        : this("") { }

    public static readonly ErrorFunctionSymbol Unnamed = new();

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];
    public override TypeSymbol ReturnType { get; }
    public override TypeSymbol? ReceiverType => null;
    public override ImmutableArray<ParameterSymbol> Parameters => [];
    public override FunctionKind Kind => FunctionKind.Ordinary;
    internal override AbiKind AbiKind => AbiKind.Internal;
    public override bool IsExtern => false;
}
