// @file MirEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Prism.Core.BoundTree;
using Prism.Core.Mir;
using Prism.Core.Symbols;

namespace Prism.Core.Codegen.Mir;

internal readonly record struct MirLoopTargets(MirBlockId BreakTarget, MirBlockId ContinueTarget);

internal sealed class MirEmissionContext(
    FunctionSymbol function,
    BoundStatement? body,
    MirFunctionBuilder builder,
    MirTypeMapper typeMapper,
    IReadOnlyDictionary<VariableSymbol, MirGlobal> globals,
    IReadOnlyDictionary<FunctionSymbol, MirFunctionId> functionIds
)
{
    private readonly Dictionary<Symbol, MirLocal> _locals = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LabelSymbol, MirLoopTargets> _loopTargets = new(
        ReferenceEqualityComparer.Instance
    );

    public FunctionSymbol Function { get; } = function;

    public BoundStatement? Body { get; } = body;

    public MirBasicBlockBuilder CurrentBlock { get; private set; } = null!;

    public MirFunctionId GetReferencedFunction(FunctionSymbol symbol)
    {
        return functionIds.TryGetValue(symbol, out var id)
            ? id
            : throw new KeyNotFoundException("Function not a part of this module");
    }

    public MirBasicBlockBuilder AddBlock(string name)
    {
        return builder.AddBlock(name);
    }

    public void AddBlock(MirBasicBlockBuilder block)
    {
        builder.AddBlock(block);
    }

    public MirBasicBlockBuilder AddDetachedBlock(string name)
    {
        return builder.AddDetachedBlock(name);
    }

    public void SetCurrentBlock(MirBasicBlockBuilder block)
    {
        CurrentBlock = block;
    }

    public MirLocal BindParameter(ParameterSymbol parameter)
    {
        var local = builder.AddLocal(
            parameter.Name,
            typeMapper.Map(parameter.Type),
            MirLocalKind.Parameter,
            isMutable: parameter.IsMutable
        );

        _locals.Add(parameter, local);
        return local;
    }

    public MirLocal BindLocal(VariableSymbol variable)
    {
        var local = builder.AddLocal(
            variable.Name,
            typeMapper.Map(variable.Type),
            MirLocalKind.User,
            isMutable: variable.IsMutable
        );

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

    public MirLocal CreateTemp(MirType type, string name = "tmp")
    {
        return builder.AddLocal(name, type, MirLocalKind.Temporary);
    }

    public MirGlobal GetGlobal(VariableSymbol symbol)
    {
        return globals.GetValueOrDefault(symbol)
            ?? throw new KeyNotFoundException("Symbol has not been bound to a MIR global.");
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
