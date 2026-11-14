using System.Collections.Immutable;
using Prism.Compiler.Symbols;
using Prism.Semantic;
using Prism.Semantic.Symbols;
using Prism.Syntax;

namespace Prism.Compiler.Binding;

internal sealed class BlockBinder(Binder next, BlockSyntax block) : LocalScopeBinder(next)
{
    protected override SyntaxNode? ScopeDesignator => block;

    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return BuildLocalVariables(block.Statements, this);
    }
}
