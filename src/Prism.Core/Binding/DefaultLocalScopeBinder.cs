using System.Collections.Immutable;
using Prism.Core.Symbols;

namespace Prism.Core.Binding;

internal sealed class DefaultLocalScopeBinder(Binder next) : LocalScopeBinder(next)
{
    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return [];
    }
}
