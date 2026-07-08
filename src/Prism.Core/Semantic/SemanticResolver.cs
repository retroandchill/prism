// @file SemanticResolver.cs
// 
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Prism.Core.Ast;
using Prism.Core.Diagnostics;
using Prism.Core.Semantic.Binding;
using Prism.Core.Semantic.Symbols;
using ZLinq;

namespace Prism.Core.Semantic;

internal sealed class SemanticResolver(SemanticModel model, SemanticBinder binder)
{
    private readonly ConcurrentDictionary<ValueSymbol, AsyncResolutionState<TypeSymbol>> _valueTypeSlots = new(
        ReferenceEqualityComparer.Instance
    );
    private readonly ConcurrentDictionary<VariableSymbol, AsyncResolutionState<BoundExpression>> _variableInitializers = new(
        ReferenceEqualityComparer.Instance
    );
    
    private readonly ConcurrentDictionary<CallableSymbol, AsyncResolutionState<TypeSymbol>> _functionReturnTypes = new(
        ReferenceEqualityComparer.Instance
    );

    public ResolvedSemanticState Complete()
    {
        return new ResolvedSemanticState
        {
            ValueTypes = _valueTypeSlots.ToFrozenDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Result),
            Initializers = _variableInitializers.ToFrozenDictionary(kvp => kvp.Key,
                kvp => kvp.Value.Result)
        };
    }
    
    internal TypeSymbol? GetValueTypeUnsafe(ValueSymbol valueSymbol)
    {
        return _valueTypeSlots.TryGetValue(valueSymbol, out var slot) && slot.IsCompleted ? slot.Result : null;
    }

    public ValueTask<TypeSymbol> ResolveValueTypeAsync(ValueSymbol symbol, BindingContext context,
        CancellationToken cancellationToken = default)
    {
        EnsureSymbolResolvable(symbol.Declaration);
        var key = new ResolutionKey(symbol, ResolutionKind.ValueType);

        if (context.Resolution.Contains(key))
        {
            context.Diagnostics.CyclicSymbolDefinition(symbol.Declaration.Range, symbol.Name, context.Resolution.CollectCycle(key));
            return ValueTask.FromResult<TypeSymbol>(model.ErrorTypeSymbol);
        }
        
        var slot = _valueTypeSlots.GetOrAdd(symbol, static _ => new AsyncResolutionState<TypeSymbol>());
        
        return new ValueTask<TypeSymbol>(
            slot.GetOrStart(() => ResolveValueTypeCoreAsync(symbol, context with { Resolution = context.Resolution.Push(key) }, cancellationToken))
        );
    }
    
    private static void EnsureSymbolResolvable([NotNull] DeclarationSyntax? syntax)
    {
        if (syntax is null)
            throw new InvalidOperationException("Symbol must be resolvable");
    }
    
    private async Task<TypeSymbol> ResolveValueTypeCoreAsync(ValueSymbol symbol, BindingContext context,
        CancellationToken cancellationToken)
    {

        return symbol switch
        {
            VariableSymbol variable => await ResolveVariableValueTypeAsync(variable, context, cancellationToken),
            ParameterSymbol parameter => await ResolveParameterValueTypeAsync(parameter, cancellationToken),
            _ => throw new InvalidOperationException("Unknown symbol type"),
        };
    }
    
    private async ValueTask<TypeSymbol> ResolveVariableValueTypeAsync(VariableSymbol symbol, BindingContext context,
        CancellationToken cancellationToken)
    {        
        EnsureSymbolResolvable(symbol.Declaration);

        var syntax = (VariableDeclarationSyntax)symbol.Declaration;
        if (syntax.Type is not null)
        {
            return await ResolveExplicitTypeAsync(syntax.Type, cancellationToken);
        }

        var boundInitializer = await ResolveVariableInitializerAsync(symbol, context, cancellationToken);
        // If this returns null, then that means the function had no initializer and thus we can't determine the type,
        // because there was neither an explicit type nor an initializer.
        if (boundInitializer is not null) return boundInitializer.Type;

        context.Diagnostics.VariableWithoutTypeOrInitializer(syntax.Range, symbol.Name);
        return model.ErrorTypeSymbol;

    }
    
    private ValueTask<TypeSymbol> ResolveParameterValueTypeAsync(ParameterSymbol symbol, CancellationToken cancellationToken)
    {
        EnsureSymbolResolvable(symbol.Declaration);

        
        var syntax = (ParameterDeclarationSyntax)symbol.Declaration;
        return ResolveExplicitTypeAsync(syntax.Type, cancellationToken);
    }

    private ValueTask<TypeSymbol> ResolveExplicitTypeAsync(TypeSyntax typeSyntax, CancellationToken cancellationToken)
    {
        return typeSyntax switch
        {
            BuiltInTypeSyntax builtIn => ValueTask.FromResult<TypeSymbol>(model.BuiltInTypes.GetSymbol(builtIn.Type)),
            NamedTypeSyntax => throw new NotImplementedException(),
            _ => throw new InvalidOperationException("Unknown symbol type"),
        };
    }

    public ValueTask<BoundExpression?> ResolveVariableInitializerAsync(VariableSymbol symbol, BindingContext context,
        CancellationToken cancellationToken = default)
    {
        EnsureSymbolResolvable(symbol.Declaration);
        var syntax = (VariableDeclarationSyntax)symbol.Declaration;
        
        if (syntax.Initializer is null)
        {
            return new ValueTask<BoundExpression?>((BoundExpression?)null);
        }
        
        var slot = _variableInitializers.GetOrAdd(symbol, static _ => new AsyncResolutionState<BoundExpression>());
        return new ValueTask<BoundExpression?>(slot.GetOrStart(async () =>
        {
            var declarationScope = model.GetDeclarationScope(symbol.Declaration);
            var localScope = new LocalScope(declarationScope, context.Diagnostics);
            var targetType = syntax.Type is not null ? _valueTypeSlots[symbol].Result : null;
            return await binder.BindAsync(syntax.Initializer, localScope, context, targetType, cancellationToken);
        })!);
    }
    
    public ValueTask<TypeSymbol> ResolveFunctionReturnTypeAsync(CallableSymbol symbol, BindingContext context,
        CancellationToken cancellationToken = default)
    {
        EnsureSymbolResolvable(symbol.Declaration);
        var key = new ResolutionKey(symbol, ResolutionKind.ValueType);

        if (context.Resolution.Contains(key))
        {
            context.Diagnostics.CyclicSymbolDefinition(symbol.Declaration.Range, symbol.Name, context.Resolution.CollectCycle(key));
            return ValueTask.FromResult<TypeSymbol>(model.ErrorTypeSymbol);
        }
        
        var slot = _functionReturnTypes.GetOrAdd(symbol, static _ => new AsyncResolutionState<TypeSymbol>());
        
        return new ValueTask<TypeSymbol>(
            slot.GetOrStart(() => ResolveReturnTypeCoreAsync(symbol, context with { Resolution = context.Resolution.Push(key) }, cancellationToken))
        );
    }
    
    private async Task<TypeSymbol> ResolveReturnTypeCoreAsync(CallableSymbol symbol, BindingContext context,
        CancellationToken cancellationToken)
    {
        EnsureSymbolResolvable(symbol.Declaration);
        var syntax = (FunctionDeclarationSyntax)symbol.Declaration;
        if (syntax.ReturnType is not null)
        {
            return await ResolveExplicitTypeAsync(syntax.ReturnType, cancellationToken);
        }
        
        // We need to resolve the bodies in this instance
        throw new NotImplementedException();
    }
}