// @file MirEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Mir;
using Prism.Core.Symbols;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Codegen.Mir;

internal readonly record struct MirLoopTargets(
    MirBlockId BreakTarget,
    MirBlockId ContinueTarget,
    MirScope EnclosingScope
);

internal sealed class MirScope(MirScope? parent = null)
{
    public MirScope? Parent { get; } = parent;

    public List<VariableSymbol> Locals { get; } = [];
}

internal sealed class MirEmissionContext
{
    private readonly Dictionary<Symbol, MirLocal> _locals = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LabelSymbol, MirLoopTargets> _loopTargets = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly MirFunctionBuilder _builder;
    private MirScope _scope = new();
    private readonly MirScope _functionScope;

    public MirEmissionContext(MirFunctionBuilder builder)
    {
        _builder = builder;
        foreach (var local in _builder.Locals)
        {
            if (local.Symbol is not null)
            {
                _locals.Add(local.Symbol, local);
            }
        }

        _functionScope = _scope;
    }

    public MirBasicBlockBuilder CurrentBlock { get; private set; } = null!;

    public MirBasicBlockBuilder AddBlock(string name)
    {
        return _builder.AddBlock(name);
    }

    public void AddBlock(MirBasicBlockBuilder block)
    {
        _builder.AddBlock(block);
    }

    public MirBasicBlockBuilder AddDetachedBlock(string name)
    {
        return _builder.AddDetachedBlock(name);
    }

    public void SetCurrentBlock(MirBasicBlockBuilder block)
    {
        CurrentBlock = block;
    }

    public void EnterScope()
    {
        _scope = new MirScope(_scope);
    }

    public void ExitScope()
    {
        if (!CurrentBlock.IsTerminated)
        {
            EmitScopeCleanup(_scope);
        }

        _scope = _scope.Parent.RequireNonNull();
    }

    public void EmitExitTo(MirScope target)
    {
        for (var scope = _scope; scope != target; scope = scope.Parent.RequireNonNull())
        {
            EmitScopeCleanup(scope);
        }
    }

    public void EmitEarlyReturnExit()
    {
        for (var scope = _scope; scope is not null; scope = scope.Parent)
        {
            EmitScopeCleanup(scope);
        }
    }

    private void EmitScopeCleanup(MirScope scope)
    {
        foreach (var variable in scope.Locals.AsValueEnumerable().Reverse())
        {
            var local = GetLocal(variable);
            CurrentBlock.AddInstruction(new MirStorageDeadInstruction(local.Id));
        }
    }

    public MirLocal BindLocal(VariableSymbol variable)
    {
        var local = _builder.AddLocalVariable(variable);

        _locals.Add(variable, local);
        _scope.Locals.Add(variable);
        CurrentBlock.AddInstruction(new MirStorageLiveInstruction(local.Id));
        return local;
    }

    public MirLocal GetLocal(Symbol symbol)
    {
        return TryGetLocal(symbol)
            ?? throw new KeyNotFoundException("Symbol has not been bound to a MIR local.");
    }

    public MirLocal? TryGetLocal(Symbol symbol)
    {
        return _locals.GetValueOrDefault(symbol);
    }

    public MirLocal CreateTemp(TypeSymbol type, string name = "tmp")
    {
        return _builder.AddTemporary(name, type);
    }

    public void BindLoop(LabelSymbol label, MirBlockId breakTarget, MirBlockId continueTarget)
    {
        _loopTargets.Add(label, new MirLoopTargets(breakTarget, continueTarget, _scope));
    }

    public MirLoopTargets GetLoopTargets(LabelSymbol label)
    {
        return _loopTargets.TryGetValue(label, out var targets)
            ? targets
            : throw new KeyNotFoundException("Loop label has not been bound.");
    }
}
