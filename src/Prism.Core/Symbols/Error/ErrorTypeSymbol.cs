using System.Collections.Immutable;
using Prism.Core.Syntax;
using Prism.Diagnostics;

namespace Prism.Core.Symbols.Error;

internal sealed class ErrorTypeSymbol : NamedTypeSymbol
{
    internal ErrorTypeSymbol(string name, Symbol? containingSymbol = null)
    {
        IsErrorType = true;
        Name = name;
        ContainingSymbol = containingSymbol;
    }

    private ErrorTypeSymbol()
        : this("<Error>") { }

    public override string Name { get; }
    public override Symbol? ContainingSymbol { get; }

    public override NamedTypeKind Kind => NamedTypeKind.Class;

    public static readonly ErrorTypeSymbol Unnamed = new();

    public override ImmutableArray<Location> Locations => [];
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => [];

    public override ImmutableArray<Symbol> GetMembers() => [];

    public override ImmutableArray<Symbol> GetMembers(string name) => [];

    public override bool IsDynamicallySized => false;
    public override bool IsDefaultConstructible => false;
}
