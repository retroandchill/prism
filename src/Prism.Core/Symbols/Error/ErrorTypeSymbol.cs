using System.Collections.Immutable;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorTypeSymbol : NamedTypeSymbol
{
    internal ErrorTypeSymbol(string name, Symbol? containingSymbol = null)
        : base(name, containingSymbol) { }

    private ErrorTypeSymbol()
        : this("") { }

    public static readonly ErrorTypeSymbol Unnamed = new();

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];
}
