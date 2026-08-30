using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Source;

internal sealed class SourceFunctionSymbol : FunctionSymbol
{
    internal SourceFunctionSymbol(
        string name,
        Symbol? containingSymbol,
        FunctionDeclarationSyntax syntax
    )
        : base(name, containingSymbol) { }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }
    public override TypeSymbol ReturnType { get; }
    public override ImmutableArray<ParameterSymbol> Parameters { get; }
}
