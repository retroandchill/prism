// @file FunctionEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LLVMSharp.Interop;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen;

internal readonly record struct LoopLabels(LLVMBasicBlockRef Break, LLVMBasicBlockRef Continue);

internal sealed class FunctionEmissionContext(LLVMValueRef function)
{
    private readonly struct ScopeFrame()
    {
        public Dictionary<Symbol, LLVMValueRef> Symbols { get; } =
            new(ReferenceEqualityComparer.Instance);
        public Dictionary<LabelSymbol, LoopLabels> Labels { get; } =
            new(ReferenceEqualityComparer.Instance);
    }

    private readonly List<ScopeFrame> _scopeFrames = [new()];

    public LLVMValueRef Function { get; } = function;

    public void PushScope()
    {
        _scopeFrames.Add(new ScopeFrame());
    }

    public void PopScope()
    {
        _scopeFrames.RemoveAt(_scopeFrames.Count - 1);
    }

    public void BindStorage(Symbol symbol, LLVMValueRef value)
    {
        _scopeFrames[^1].Symbols.Add(symbol, value);
    }

    public LLVMValueRef? LookupStorage(Symbol symbol)
    {
        foreach (var storage in _scopeFrames.AsValueEnumerable().Reverse())
        {
            if (storage.Symbols.TryGetValue(symbol, out var value))
                return value;
        }

        return null;
    }

    public void BindLabel(
        LabelSymbol label,
        LLVMBasicBlockRef breakBlock,
        LLVMBasicBlockRef continueBlock
    )
    {
        _scopeFrames[^1].Labels.Add(label, new LoopLabels(breakBlock, continueBlock));
    }

    public LoopLabels? LookupLabels(LabelSymbol label)
    {
        foreach (var storage in _scopeFrames.AsValueEnumerable().Reverse())
        {
            if (storage.Labels.TryGetValue(label, out var value))
                return value;
        }

        return null;
    }
}
