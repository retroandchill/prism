// @file SemanticModel.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;

namespace Prism.Core.Semantic;

public sealed class SemanticModel
{
    private readonly ConcurrentDictionary<DeclarationSyntax, Symbol> _symbolCache = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly ConcurrentDictionary<DeclarationSyntax, DeclarationScope> _declaringScopes =
        new(ReferenceEqualityComparer.Instance);
    private readonly ConcurrentDictionary<DeclarationSyntax, DeclarationScope> _ownedScopes = new(
        ReferenceEqualityComparer.Instance
    );

    public DeclarationScope GlobalScope { get; } = new();

    public Symbol? GetSymbol(DeclarationSyntax node)
    {
        return _symbolCache.GetValueOrDefault(node);
    }

    public DeclarationScope? GetOwnedScope(DeclarationSyntax node)
    {
        return _ownedScopes.GetValueOrDefault(node);
    }

    public DeclarationScope GetDeclarationScope(DeclarationSyntax node)
    {
        return _declaringScopes.GetValueOrDefault(node) ?? throw new KeyNotFoundException();
    }

    internal void AddCompilationUnit(CompilationUnitSyntax compilationUnit)
    {
        foreach (var declaration in compilationUnit.Declarations)
            AddSymbol(declaration, GlobalScope);
    }

    private void AddSymbol(DeclarationSyntax declaration, DeclarationScope scope)
    {
        switch (declaration)
        {
            case VariableDeclarationSyntax variableDeclaration:
                AddSymbol(variableDeclaration, v => new VariableSymbol(v), scope);
                break;
            case ParameterDeclarationSyntax parameterDeclaration:
                AddSymbol(parameterDeclaration, p => new ParameterSymbol(p), scope);
                break;
            case FunctionDeclarationSyntax functionDeclaration:
                AddSymbol(functionDeclaration, scope);
                break;
        }
    }

    private TSymbol AddSymbol<TSymbol, TSyntax>(TSyntax syntax, Func<TSyntax, TSymbol> createSymbol, DeclarationScope scope)
        where TSymbol : Symbol
        where TSyntax : DeclarationSyntax
    {
        var symbol = createSymbol(syntax);
        scope.AddDeclaration(symbol);
        _symbolCache[syntax] = symbol;
        _declaringScopes[syntax] = scope;
        return symbol;
    }
    
    private void AddSymbol(FunctionDeclarationSyntax declaration, DeclarationScope scope)
    {
        var functionScope = new DeclarationScope(declaration, scope);
        _ownedScopes[declaration] = functionScope;
        AddSymbol(declaration, f => new FunctionSymbol(f, p => AddSymbol(p, x => new ParameterSymbol(x), functionScope)), scope);
        
    }
}