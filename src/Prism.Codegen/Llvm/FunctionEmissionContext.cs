// @file FunctionEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using LLVMSharp.Interop;
using Prism.Mir;
using Prism.Mir.Analysis;

namespace Prism.Codegen.Llvm;

internal sealed class FunctionEmissionContext(
    Function function,
    LLVMValueRef llvmFunction,
    FrozenDictionary<Local, LocalClassification> localClassification
)
{
    private readonly Dictionary<Local, LLVMValueRef> _locals = [];
    private readonly Dictionary<BasicBlock, LLVMBasicBlockRef> _blocks = [];

    public Function MirFunction { get; } = function;

    public LLVMValueRef LlvmFunction { get; } = llvmFunction;
    public FrozenDictionary<Local, LocalClassification> LocalClassification { get; } =
        localClassification;

    public void BindLocal(Local local, LLVMValueRef value)
    {
        _locals[local] = value;
    }

    public LLVMValueRef LookupLocal(Local local)
    {
        return _locals.TryGetValue(local, out var value)
            ? value
            : throw new KeyNotFoundException($"Invalid local ID: {local.Id}");
    }

    public void BindBlock(BasicBlock block, LLVMBasicBlockRef blockRef)
    {
        _blocks.Add(block, blockRef);
    }

    public LLVMBasicBlockRef LookupBlock(BasicBlock block)
    {
        return _blocks.TryGetValue(block, out var value)
            ? value
            : throw new KeyNotFoundException("Invalid block ID");
    }
}
