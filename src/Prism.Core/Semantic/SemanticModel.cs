// @file SemanticModel.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Prism.Core.Ast;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic.Binding;
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

    private readonly SemanticBinder _semanticBinder;
    private readonly SemanticResolver _semanticResolver;
    private ResolvedSemanticState? _resolvedSemanticState;

    private readonly Compilation _compilation;
    
    public TargetPlatform TargetPlatform => _compilation.TargetPlatform;
    
    internal BuiltInTypeSet BuiltInTypes { get; }

    internal ErrorTypeSymbol ErrorTypeSymbol { get; }
    internal UnresolvedValueSymbol UnresolvedValueSymbol { get; }

    public DeclarationScope GlobalScope { get; } = new();

    internal SemanticModel(Compilation compilation)
    {
        _compilation = compilation;
        BuiltInTypes = new BuiltInTypeSet(compilation);
        ErrorTypeSymbol = new ErrorTypeSymbol(compilation);
        UnresolvedValueSymbol = new UnresolvedValueSymbol(compilation);
        _semanticBinder = new SemanticBinder(this);
        _semanticResolver = new SemanticResolver(this, _semanticBinder);
    }

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

    private void AddSymbol(DeclarationSyntax declaration, DeclarationScope scope, Symbol? containingSymbol = null)
    {
        switch (declaration)
        {
            case VariableDeclarationSyntax variableDeclaration:
                AddSymbol(variableDeclaration, (v, c) => new VariableSymbol(v, _compilation, containingSymbol: c), scope,
                    containingSymbol);
                break;
            case ParameterDeclarationSyntax parameterDeclaration:
                AddSymbol(parameterDeclaration, (p, c) => new ParameterSymbol(p, _compilation, containingSymbol: c),
                    scope, containingSymbol);
                break;
            case FunctionDeclarationSyntax functionDeclaration:
                AddSymbol(functionDeclaration, scope, containingSymbol);
                break;
        }
    }

    private TSymbol AddSymbol<TSymbol, TSyntax>(TSyntax syntax, Func<TSyntax, Symbol?, TSymbol> createSymbol,
        DeclarationScope scope, Symbol? containingSymbol)
        where TSymbol : Symbol
        where TSyntax : DeclarationSyntax
    {
        var symbol = createSymbol(syntax, containingSymbol);
        scope.AddDeclaration(symbol);
        _symbolCache[syntax] = symbol;
        _declaringScopes[syntax] = scope;
        return symbol;
    }

    private void AddSymbol(FunctionDeclarationSyntax declaration, DeclarationScope scope, Symbol? containingSymbol)
    {
        var functionScope = new DeclarationScope(declaration, scope);
        _ownedScopes[declaration] = functionScope;
        AddSymbol(declaration,
            (f, c) => new FunctionSymbol(f, _compilation,
                (p, s) => AddSymbol(p, 
                    (x, c1) => new ParameterSymbol(x, _compilation, c1), functionScope, s), containingSymbol: c), scope,
            containingSymbol);
    }

    internal async ValueTask<DiagnosticBag> BindSymbolsAsync(CancellationToken cancellationToken = default)
    {
        var topLevelBag = new DiagnosticBag();
        await foreach (var task in Task
                           .WhenEach(_symbolCache.Keys.Select(x =>
                               BindSymbolAsync(_symbolCache[x], cancellationToken)))
                           .WithCancellation(cancellationToken))
        {
            var result = task.Result;
            foreach (var diagnostic in result)
            {
                topLevelBag.Report(diagnostic);
            }
        }

        _resolvedSemanticState = _semanticResolver.Complete();
        return topLevelBag;
    }

    private async Task<DiagnosticBag> BindSymbolAsync(Symbol symbol, CancellationToken cancellationToken = default)
    {
        var bag = new DiagnosticBag();
        var context = new BindingContext(bag, ResolutionContext.Empty);
        switch (symbol)
        {
            case VariableSymbol variableSymbol:
                await ResolveValueTypeAsync(variableSymbol, context, cancellationToken);
                await ResolveVariableInitializerAsync(variableSymbol, context, cancellationToken);
                break;
            case ParameterSymbol parameterSymbol:
                await ResolveValueTypeAsync(parameterSymbol, context, cancellationToken);
                break;
            default:
                throw new NotImplementedException();
        }
        
        return bag;
    }

    internal TypeSymbol GetValueType(ValueSymbol valueSymbol)
    {
        return _resolvedSemanticState is not null ? _resolvedSemanticState.ValueTypes[valueSymbol] : throw new InvalidOperationException("Type resolution not complete");
    }

    internal ValueTask<TypeSymbol> ResolveValueTypeAsync(ValueSymbol valueSymbol,
        BindingContext context,
        CancellationToken cancellationToken = default)
    {
        return _semanticResolver.ResolveValueTypeAsync(valueSymbol, context, cancellationToken);
    }
    
    internal BoundExpression? GetVariableInitializer(VariableSymbol symbol)
    {
        return _resolvedSemanticState is not null ? _resolvedSemanticState.Initializers.GetValueOrDefault(symbol) : throw new InvalidOperationException("Type resolution not complete");
    }

    internal ValueTask<BoundExpression?> ResolveVariableInitializerAsync(VariableSymbol valueSymbol, BindingContext context,
        CancellationToken cancellationToken = default)
    {
        return _semanticResolver.ResolveVariableInitializerAsync(valueSymbol, context, cancellationToken);
    }
}