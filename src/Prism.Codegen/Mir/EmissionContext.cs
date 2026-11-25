// @file EmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Prism.Mir;
using Prism.Semantic.Symbols;
using Prism.Utils;
using ZLinq;

namespace Prism.Codegen.Mir;

internal readonly record struct LoopTargets(
    BasicBlockBuilder BreakTarget,
    BasicBlockBuilder ContinueTarget,
    Scope EnclosingScope
);

internal sealed class Scope(Scope? parent = null)
{
    public Scope? Parent { get; } = parent;

    public List<VariableSymbol> Locals { get; } = [];
}

internal readonly struct ExpressionLifetime(EmissionContext context)
{
    private readonly List<Local> _temporaries = [];

    private BasicBlockBuilder CurrentBlock => context.CurrentBlock;

    public void AddTemporary(Local local)
    {
        _temporaries.Add(local);
    }

    public void EmitCleanup()
    {
        Debug.Assert(
            !CurrentBlock.IsTerminated,
            "Cannot clean up temporaries after block is terminated"
        );
        foreach (var temporary in _temporaries.AsValueEnumerable().Reverse())
        {
            CurrentBlock.AddStorageDead(temporary);
        }
    }
}

internal sealed class EmissionContext
{
    private readonly Dictionary<ValueSymbol, (Local Local, PlaceBase Place)> _locals = [];
    private readonly Dictionary<LabelSymbol, LoopTargets> _loopTargets = [];

    public ExecutableBodyBuilder Builder { get; }
    private Scope _scope = new();

    public PlaceBase? ReturnPlace { get; }

    public PlaceBase? ReceiverPlace { get; }

    public EmissionContext(FunctionBuilder builder)
    {
        Builder = builder.ExecutableBody;

        if (builder.ReturnValueLocal is not null)
        {
            ReturnPlace = new LocalPlace(builder.ReturnValueLocal);
        }

        if (builder.ReceiverLocal is not null)
        {
            ReceiverPlace = new LocalPlace(builder.ReceiverLocal);
        }

        foreach (var (parameter, local) in builder.Parameters)
        {
            _locals.Add(parameter, (local, new LocalPlace(local)));
        }

        CurrentBlock = Builder.AddBasicBlock("entry");
        Builder.EntryBlock = CurrentBlock;
    }

    public BasicBlockBuilder CurrentBlock { get; set; }

    public BasicBlockBuilder AddBlock(string name)
    {
        return Builder.AddBasicBlock(name);
    }

    public void AddBlock(BasicBlockBuilder block)
    {
        Builder.AddExistingBlock(block);
    }

    public BasicBlockBuilder AddDetachedBlock(string name)
    {
        return Builder.AddDetachedBlock(name);
    }

    public ExpressionLifetime CreateExpressionLifetime() => new(this);

    public void EnterScope()
    {
        _scope = new Scope(_scope);
    }

    public void ExitScope()
    {
        if (!CurrentBlock.IsTerminated)
        {
            EmitScopeCleanup(_scope);
        }

        _scope = _scope.Parent.RequireNonNull();
    }

    public void EmitExitTo(Scope target)
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

    private void EmitScopeCleanup(Scope scope)
    {
        foreach (var variable in scope.Locals.AsValueEnumerable().Reverse())
        {
            var (local, _) = GetLocal(variable);
            // TODO: We probably want to cache this place
            CurrentBlock.AddStorageDead(local);
        }
    }

    public (Local, PlaceBase) BindLocal(VariableSymbol variable)
    {
        var local = Builder.AddLocal(variable);

        var result = (local, new LocalPlace(local));
        _locals.Add(variable, result);
        _scope.Locals.Add(variable);
        CurrentBlock.AddStorageLive(local);
        return result;
    }

    public (Local, PlaceBase) CreateTemporary(TypeSymbol type, ExpressionLifetime lifetime)
    {
        var local = Builder.AddLocal(type);
        var result = (local, new LocalPlace(local));
        lifetime.AddTemporary(local);
        CurrentBlock.AddStorageLive(local);
        return result;
    }

    public (Local Local, PlaceBase Place) GetLocal(ValueSymbol symbol)
    {
        return TryGetLocal(symbol)
            ?? throw new KeyNotFoundException("Symbol has not been bound to a MIR local.");
    }

    public (Local Local, PlaceBase Place)? TryGetLocal(ValueSymbol symbol)
    {
        return _locals.GetValueOrNull(symbol);
    }

    public void BindLoop(
        LabelSymbol label,
        BasicBlockBuilder breakTarget,
        BasicBlockBuilder continueTarget
    )
    {
        _loopTargets.Add(label, new LoopTargets(breakTarget, continueTarget, _scope));
    }

    public LoopTargets GetLoopTargets(LabelSymbol label)
    {
        return _loopTargets.TryGetValue(label, out var targets)
            ? targets
            : throw new KeyNotFoundException("Loop label has not been bound.");
    }
}
