using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class BlockBinder(Binder next, BlockSyntax block) : LocalScopeBinder(next)
{
    protected override SyntaxNode? ScopeDesignator => block;

    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return BuildLocalVariables(block.Statements, this);
    }
}
