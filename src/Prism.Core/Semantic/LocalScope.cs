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
    private readonly CompilationContext _context;

    public LocalScope(DeclarationScope enclosingScope, CompilationContext context)
    {
        _enclosingScope = enclosingScope;
        _context = context;
    }

    public LocalScope(LocalScope parent)
    {
        _enclosingScope = parent;
        _context = parent._context;
    }

    public LocalScope CreateChildScope() => new(this);

    public void DefineLocal(Symbol symbol)
    {
        if (_locals.ContainsKey(symbol.Name))
        {
            // Report the duplicate symbol but continue processing with the new symbol definition
            _context.ReportDiagnostic(new Diagnostic
            {
                Descriptor = SemanticDiagnostics.DuplicateSymbolDefinition,
                Range = symbol.Declaration?.Range ?? SourceRange.Empty,
                Arguments = [symbol.Name],
            });
        }

        _locals[symbol.Name] = symbol;
    }
}
