using System.Collections.Immutable;
using Prism.Compiler.Symbols;
using Prism.Semantic;

namespace Prism.Compiler.Binding;

internal sealed class DefaultLocalScopeBinder(Binder next) : LocalScopeBinder(next)
{
    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return [];
    }
}
