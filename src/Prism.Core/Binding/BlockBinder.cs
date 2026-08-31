using System.Collections.Immutable;
using System.Diagnostics;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class BlockBinder(Binder next, BlockSyntax block) : LocalScopeBinder(next)
{
    public override SyntaxNode? ScopeDesignator => block;

    public override ImmutableArray<VariableSymbol> GetDeclaredLocalVariablesForScope(
        SyntaxNode designator
    )
    {
        Debug.Assert(ReferenceEquals(designator, block));
        return LocalVariables;
    }

    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return BuildLocalVariables(block.Statements, this);
    }
}
