using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Source;

internal abstract class SourceVariableSymbol : VariableSymbol
{
    private readonly VariableDeclarationSyntax _syntax;

    protected SourceVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol)
    {
        _syntax = syntax;
    }
}

internal sealed class SourceGlobalVariableSymbol : SourceVariableSymbol
{
    public SourceGlobalVariableSymbol(
        string name,
        Symbol? containingSymbol,
        VariableDeclarationSyntax syntax
    )
        : base(name, containingSymbol, syntax) { }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }
    public override TypeSymbol Type { get; }
    public override bool IsMutable { get; }
    public override bool IsGlobal { get; }
    public override bool HasInitializer { get; }
    public override ConstantValue? ConstantValue { get; }
}
