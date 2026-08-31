using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Binding;

internal sealed class MemberContainerBinder(
    Binder next,
    MemberContainerSymbol symbol,
    DeclarationSyntax syntax
) : Binder(next)
{
    public override SyntaxNode ScopeDesignator => syntax;
    public override Symbol ContainingSymbol => symbol;
    private ImmutableArray<NamespaceSymbol> _usingNamespaces;

    protected override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        LookupContext context
    )
    {
        if (LookupQualifiedName(name, symbol, options) is { IsViable: true } result)
            return result;

        var found = GetUsingNamespaces()
            .AsValueEnumerable()
            .SelectMany(x => LookupQualifiedName(name, x, options).Symbols.AsValueEnumerable())
            .ToImmutableArray();
        return MakeLookupResult(found, options);
    }

    private ImmutableArray<NamespaceSymbol> GetUsingNamespaces()
    {
        if (!_usingNamespaces.IsDefault)
            return _usingNamespaces;

        ImmutableInterlocked.InterlockedCompareExchange(
            ref _usingNamespaces,
            ComputeUsingNamespaces(),
            default
        );
        return _usingNamespaces;
    }

    private ImmutableArray<NamespaceSymbol> ComputeUsingNamespaces()
    {
        return syntax switch
        {
            NamespaceDeclarationSyntax ns => BuildUsingNamespaces(ns.Usings),
            _ => [],
        };
    }
}
