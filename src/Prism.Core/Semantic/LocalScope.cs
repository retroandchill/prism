// @file TransientScope.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Parse;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

public sealed class LocalScope
{
    private union ParentScope(DeclarationScope, LocalScope);
    private readonly ParentScope _enclosingScope;

    private readonly Dictionary<Name, Symbol> _locals = new();
    private readonly BindingContext _bindingContext;

    public LocalScope(DeclarationScope enclosingScope, BindingContext bindingContext)
    {
        _enclosingScope = enclosingScope;
        _bindingContext = bindingContext;
    }

    public LocalScope(LocalScope parent)
    {
        _enclosingScope = parent;
        _bindingContext = parent._bindingContext;
    }

    public LocalScope CreateChildScope() => new(this);

    public void DefineLocal(Symbol symbol)
    {
        if (_locals.ContainsKey(symbol.Name))
        {
            // Report the duplicate symbol but continue processing with the new symbol definition
            _bindingContext.ReportDiagnostic(new Diagnostic
            {
                Descriptor = SemanticDiagnostics.DuplicateSymbolDefinition,
                Range = symbol.Declaration?.Range ?? SourceRange.Empty,
                Arguments = [symbol.Name],
            });
        }

        _locals[symbol.Name] = symbol;
    }
}
