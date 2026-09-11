using Prism.Core.Compiling;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class TerminalBinder(Compilation compilation) : Binder(compilation)
{
    protected override Symbol? ContainingSymbol => null;

    protected override SyntaxNode? ScopeDesignator => null;

    public override Binder? GetBinder(SyntaxNode node) => null;

    protected override void EnsureLocals()
    {
        // Do nothing
    }

    protected override LookupResult LookupLocal(
        string name,
        LookupOptions options,
        BindingContext context
    )
    {
        return LookupResult.NotFound();
    }

    public override LabelSymbol? LookupLoopLabel(string name, BindingContext context) => null;
}
