using System.Collections.Immutable;
using Prism.Core.Compiling;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class TerminalBinder(Compilation compilation) : Binder(compilation)
{
    public override Symbol? ContainingSymbol => null;

    public override SyntaxNode? ScopeDesignator => null;

    public override Binder? GetBinder(SyntaxNode node) => null;

    public override ImmutableArray<VariableSymbol> GetDeclaredLocalVariablesForScope(
        SyntaxNode designator
    )
    {
        return [];
    }

    protected override void EnsureLocals()
    {
        // Do nothing
    }

    protected override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        LookupContext context
    )
    {
        return LookupResult.NotFound();
    }
}
