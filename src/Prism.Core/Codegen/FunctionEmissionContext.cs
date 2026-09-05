// @file FunctionEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using LLVMSharp.Interop;
using Prism.Core.Mir;
using Prism.Core.Mir.Analysis;
using Prism.Core.Symbols;
using Prism.Core.Utils;
using ZLinq;

namespace Prism.Core.Codegen;

internal sealed class FunctionEmissionContext(
    MirFunction function,
    LLVMValueRef llvmFunction,
    MirControlFlowGraph cfg,
    MirLocalClassificationAnalysis localClassification
)
{
    private readonly Dictionary<MirLocalId, LLVMValueRef> _locals = new();
    private readonly Dictionary<MirBlockId, LLVMBasicBlockRef> _blocks = new();
    private readonly Dictionary<
        MirLocalId,
        (List<LLVMValueRef> Values, List<LLVMBasicBlockRef> Blocks)
    > _phiValues = new();
    private readonly Dictionary<MirBlockId, List<MirLocalId>> _phiLocalIds = new();

    public MirFunction MirFunction { get; } = function;

    public LLVMValueRef LlvmFunction { get; } = llvmFunction;
    public MirLocalClassificationAnalysis LocalClassification { get; } = localClassification;

    public MirBlockId CurrentBlock { get; set; }

    public void BindLocal(MirLocalId local, LLVMValueRef value)
    {
        _locals[local] = value;
    }

    public LLVMValueRef LookupLocal(MirLocalId local)
    {
        return _locals.TryGetValue(local, out var value)
            ? value
            : throw new KeyNotFoundException("Invalid local ID");
    }

    public void AddPhiValue(MirLocalId local, LLVMValueRef value)
    {
        var (values, blocks) = _phiValues.GetOrAdd(local, () => ([], []));
        values.Add(value);
        blocks.Add(LookupBlock(CurrentBlock));

        var successors = cfg.Successors[CurrentBlock];
        Debug.Assert(successors.Length == 1);
        var localPhis = _phiLocalIds.GetOrAdd(successors[0], () => []);
        if (!localPhis.Contains(local))
        {
            localPhis.Add(local);
        }
    }

    public PhiLookupResult LookupPhiValues(MirLocalId local)
    {
        return _phiValues.TryGetValue(local, out var tuple)
            ? new PhiLookupResult(tuple.Values, tuple.Blocks)
            : throw new KeyNotFoundException("Invalid local ID");
    }

    public IReadOnlyList<MirLocalId> LookupPhiLocals(MirBlockId block)
    {
        return _phiLocalIds.TryGetValue(block, out var list) ? list : Array.Empty<MirLocalId>();
    }

    public void BindBlock(MirBlockId block, LLVMBasicBlockRef blockRef)
    {
        _blocks.Add(block, blockRef);
    }

    public LLVMBasicBlockRef LookupBlock(MirBlockId block)
    {
        return _blocks.TryGetValue(block, out var value)
            ? value
            : throw new KeyNotFoundException("Invalid block ID");
    }

    public readonly ref struct PhiLookupResult(
        List<LLVMValueRef> values,
        List<LLVMBasicBlockRef> blocks
    )
    {
        public ReadOnlySpan<LLVMValueRef> Values { get; } = CollectionsMarshal.AsSpan(values);
        public ReadOnlySpan<LLVMBasicBlockRef> Blocks { get; } = CollectionsMarshal.AsSpan(blocks);

        public void Deconstruct(
            out ReadOnlySpan<LLVMValueRef> values,
            out ReadOnlySpan<LLVMBasicBlockRef> blocks
        )
        {
            values = Values;
            blocks = Blocks;
        }
    }
}
