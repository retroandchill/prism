// @file MirEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Mir;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen.Mir;

internal readonly record struct MirLoopTargets(MirBlockId BreakTarget, MirBlockId ContinueTarget);

internal sealed class MirEmissionContext
{
    private readonly Dictionary<Symbol, MirLocal> _locals = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LabelSymbol, MirLoopTargets> _loopTargets = new(
        ReferenceEqualityComparer.Instance
    );

    private readonly MirFunctionBuilder _builder;

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

    public MirLocal BindLocal(VariableSymbol variable)
    {
        var local = _builder.AddLocalVariable(variable);

        _locals.Add(variable, local);
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
        _loopTargets.Add(label, new MirLoopTargets(breakTarget, continueTarget));
    }

    public MirLoopTargets GetLoopTargets(LabelSymbol label)
    {
        return _loopTargets.TryGetValue(label, out var targets)
            ? targets
            : throw new KeyNotFoundException("Loop label has not been bound.");
    }
}
