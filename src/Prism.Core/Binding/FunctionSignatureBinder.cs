using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Binding;

internal sealed class FunctionSignatureBinder(
    Binder next,
    FunctionSymbol symbol,
    FunctionDeclarationSyntax syntax
) : Binder(next)
{
    public override SyntaxNode ScopeDesignator => syntax;
    public override Symbol ContainingSymbol => symbol;

    protected override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        BindingContext context
    )
    {
        if (!options.HasFlag(LookupOptions.Value))
            return LookupResult.NotFound();

        var validParameters = symbol
            .Parameters.AsValueEnumerable()
            .Where(p => p.Name == name)
            .ToImmutableArray();
        return MakeLookupResult(
            ImmutableArray<Symbol>.CastUp(validParameters),
            LookupOptions.Value
        );
    }

    public override LabelSymbol? LookupLoopLabel(string name, BindingContext context) => null;
}
