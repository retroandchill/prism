using System.Collections.Immutable;
using Prism.Core.Symbols;
using Prism.Core.Syntax;

namespace Prism.Core.Binding;

internal sealed class ForLoopBinder(Binder next, ForStatementSyntax syntax) : LoopBinder(next)
{
    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return syntax.Declaration is not null
            ? BuildLocalVariables([syntax.Declaration], this)
            : [];
    }
}
