// @file FunctionEmissionContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LLVMSharp;
using LLVMSharp.Interop;
using Prism.Core.Symbols;
using ZLinq;

namespace Prism.Core.Codegen;

internal sealed class FunctionEmissionContext(LLVMValueRef function)
{
    private readonly List<Dictionary<Symbol, LLVMValueRef>> _scopeFrames = [CreateNewFrame()];

    public LLVMValueRef Function { get; } = function;

    public void PushScope()
    {
        _scopeFrames.Add(CreateNewFrame());
    }

    public void PopScope()
    {
        _scopeFrames.RemoveAt(_scopeFrames.Count - 1);
    }

    public void BindStorage(Symbol symbol, LLVMValueRef value)
    {
        _scopeFrames[^1].Add(symbol, value);
    }

    public LLVMValueRef? LookupStorage(Symbol symbol)
    {
        foreach (var storage in _scopeFrames.AsValueEnumerable().Reverse())
        {
            if (storage.TryGetValue(symbol, out var value))
                return value;
        }

        return null;
    }

    private static Dictionary<Symbol, LLVMValueRef> CreateNewFrame() =>
        new(ReferenceEqualityComparer.Instance);
}
