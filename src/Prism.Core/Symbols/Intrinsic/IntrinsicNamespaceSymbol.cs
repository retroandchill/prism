using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;

namespace Prism.Core.Symbols.Intrinsic;

internal sealed class IntrinsicNamespaceSymbol : NamespaceSymbol
{
    internal IntrinsicNamespaceSymbol(string name, Symbol? containingSymbol)
        : base(name, containingSymbol) { }

    public override ImmutableArray<Location> Locations { get; }
    public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

    internal ImmutableArray<Symbol> Members
    {
        get;
        set
        {
            if (!field.IsDefault)
                throw new InvalidOperationException("Members cannot be set more than once.");

            field = value;
        }
    }

    public override ImmutableArray<Symbol> GetMembers() => Members;

    public override ImmutableArray<Symbol> GetMembers(string name)
    {
        return [.. Members.Where(m => m.Name == name)];
    }

    public override NamespaceKind NamespaceKind => NamespaceKind.Assembly;
    public override Compilation? ContainingCompilation => null;
}
