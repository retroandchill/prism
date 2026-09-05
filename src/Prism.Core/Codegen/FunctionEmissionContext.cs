// @file FunctionEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using JetBrains.Annotations;
using LLVMSharp.Interop;
using Prism.Core.Mir;
using Prism.Core.Mir.Analysis;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen;

internal sealed class FunctionEmissionContext(
    LLVMValueRef function,
    MirLocalClassificationAnalysis localClassification
)
{
    private readonly Dictionary<MirLocalId, LLVMValueRef> _locals = new();
    private readonly Dictionary<MirBlockId, LLVMBasicBlockRef> _blocks = new();

    public LLVMValueRef Function { get; } = function;

    public MirLocalClassificationAnalysis LocalClassification { get; } = localClassification;

    public void BindLocal(MirLocalId local, LLVMValueRef value)
    {
        _locals.Add(local, value);
    }

    public LLVMValueRef LookupLocal(MirLocalId local)
    {
        return _locals.TryGetValue(local, out var value)
            ? value
            : throw new KeyNotFoundException("Invalid local ID");
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
