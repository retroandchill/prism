using System.Collections.Immutable;
using Prism.Compiler.Symbols;
using Prism.Semantic;
using Prism.Syntax;

namespace Prism.Compiler.Binding;

internal sealed class ForLoopBinder(Binder next, ForStatementSyntax syntax, LabelSymbol label)
    : LoopBinder(next, label)
{
    protected override ImmutableArray<VariableSymbol> BuildLocalVariables()
    {
        return syntax.Declaration is not null
            ? BuildLocalVariables([syntax.Declaration], this)
            : [];
    }
}
