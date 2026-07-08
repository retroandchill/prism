// @file TransientScope.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Diagnostics;
using Prism.Core.Parse;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Strings;

namespace Prism.Core.Semantic;

internal sealed class LocalScope(DeclarationScope enclosingScope, IDiagnosticSink diagnostics)
{
    public LocalScope? OuterLocal { get; }
    public DeclarationScope OuterDeclaration { get; } = enclosingScope;

    private readonly Dictionary<Name, ValueSymbol> _locals = new();
    private readonly IDiagnosticSink _diagnostics = diagnostics;

    public LocalScope(LocalScope parent)
        : this(parent.OuterDeclaration, parent._diagnostics)
    {
        OuterLocal = parent;
    }

    public LocalScope CreateChildScope() => new(this);

    public void DefineLocal(ValueSymbol symbol)
    {
        if (_locals.ContainsKey(symbol.Name))
        {
            // Report the duplicate symbol but continue processing with the new symbol definition
            _diagnostics.DuplicateSymbolDefinition(
                symbol.Declaration?.Range ?? TextSpan.Empty,
                symbol.Name
            );
        }

        _locals[symbol.Name] = symbol;
    }

    public ValueSymbol? GetLocal(Name name) => _locals.GetValueOrDefault(name);
}
