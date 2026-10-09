using System.Collections.Immutable;
using Prism.Core.BoundTree;
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
    protected override SyntaxNode ScopeDesignator => syntax;
    protected override Symbol ContainingSymbol => symbol;

    private BoundExpression? _implicitReceiver;

    public override BoundExpression? TryGetImplicitReceiver()
    {
        if (_implicitReceiver is not null)
            return _implicitReceiver;

        var receiver = symbol.ReceiverType;
        if (receiver is null)
            return null;

        Interlocked.CompareExchange(
            ref _implicitReceiver,
            new BoundThisExpression(Compilation, syntax, receiver),
            null
        );
        return _implicitReceiver;
    }

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
