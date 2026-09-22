// @file FunctionEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LLVMSharp.Interop;
using Prism.Core.Mir;
using Prism.Core.Mir.Analysis;

namespace Prism.Core.Codegen;

internal sealed class FunctionEmissionContext(
    MirFunction function,
    LLVMValueRef llvmFunction,
    MirLocalClassificationAnalysis localClassification
)
{
    private readonly Dictionary<MirLocalId, LLVMValueRef> _locals = new();
    private readonly Dictionary<MirValueId, LLVMValueRef> _values = new();
    private readonly Dictionary<MirBlockId, LLVMBasicBlockRef> _blocks = new();

    public MirFunction MirFunction { get; } = function;

    public LLVMValueRef LlvmFunction { get; } = llvmFunction;
    public MirLocalClassificationAnalysis LocalClassification { get; } = localClassification;

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

    public void AddValue(MirValueId id, LLVMValueRef value)
    {
        _values.Add(id, value);
    }

    public LLVMValueRef LookupValue(MirValueId id)
    {
        return _values.TryGetValue(id, out var value)
            ? value
            : throw new KeyNotFoundException("Invalid value ID");
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
}
