using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorFunctionSymbol : FunctionSymbol
{
    internal ErrorFunctionSymbol(
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
    public override ImmutableArray<ParameterSymbol> Parameters => [];
}
