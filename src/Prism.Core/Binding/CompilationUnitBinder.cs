using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;
using ZLinq;

namespace Prism.Core.Binding;

internal sealed class CompilationUnitBinder(Binder next, CompilationUnitSyntax syntax)
    : Binder(next)
{
    private ImmutableArray<NamespaceSymbol> _usingNamespaces;

    public override SyntaxNode ScopeDesignator => syntax;
    public override Symbol ContainingSymbol => Compilation.GlobalNamespace;

    protected override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        LookupContext context
    )
    {
        var found = GetUsingNamespaces()
            .AsValueEnumerable()
            .SelectMany(x => LookupQualifiedName(name, x, options).Symbols.AsValueEnumerable())
            .ToImmutableArray();

        return !found.IsEmpty
            ? MakeLookupResult(found, options)
            : LookupQualifiedName(name, Compilation.GlobalNamespace, options);
    }

    private ImmutableArray<NamespaceSymbol> GetUsingNamespaces()
    {
        if (!_usingNamespaces.IsDefault)
            return _usingNamespaces;

        Interlocked.CompareExchange(
            ref _usingNamespaces,
            BuildUsingNamespaces(syntax.Usings),
            default
        );
        return _usingNamespaces;
    }
}
