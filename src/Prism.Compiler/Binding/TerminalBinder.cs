using Prism.Compiler.BoundTree;
using Prism.Compiler.Semantic;
using Prism.Compiler.Symbols;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Binding;

internal sealed class TerminalBinder(Compilation compilation) : Binder(compilation)
{
    protected override Symbol? ContainingSymbol => null;

    protected override SyntaxNode? ScopeDesignator => null;

    public override Binder? GetBinder(SyntaxNode node) => null;

    public override BoundExpression? TryGetImplicitReceiver() => null;

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
