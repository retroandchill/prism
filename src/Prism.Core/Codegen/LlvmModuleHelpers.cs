// @file LlvmModuleHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using LLVMSharp.Interop;

namespace Prism.Core.Codegen;

internal static class LlvmModuleHelpers
{
    private const string GlobalCtorsName = "llvm.global_ctors";

    public static void AppendToGlobalCtors(
        this LLVMModuleRef module,
        LLVMValueRef function,
        int priority
    )
    {
        var context = module.Context;

        var int32Type = context.Int32Type;
        var ptrType = context.CreatePointerType(0);

        var elementType = context.GetStructType([int32Type, ptrType, ptrType], false);

        var newElement = LLVMValueRef.CreateConstStruct(
            [
                LLVMValueRef.CreateConstInt(int32Type, unchecked((ulong)priority)),
                function,
                LLVMValueRef.CreateConstNull(ptrType),
            ],
            false
        );

        LLVMValueRef[]? ctorElements = null;
        var existingCount = 0u;
        var newCount = 1u;

        var existingGlobal = module.GetNamedGlobal(GlobalCtorsName);

        LLVMValueRef newArrayInit;
        try
        {
            if (!existingGlobal.IsNull)
            {
                var initializer = existingGlobal.Initializer;
                if (!initializer.IsAConstantArray.IsNull)
                {
                    existingCount = (uint)initializer.OperandCount;
                    newCount = existingCount + 1;

                    ctorElements = ArrayPool<LLVMValueRef>.Shared.Rent((int)newCount);
                    for (var i = 0u; i < existingCount; i++)
                    {
                        ctorElements[i] = initializer.GetOperand(i);
                    }
                }
            }

            ctorElements ??= ArrayPool<LLVMValueRef>.Shared.Rent((int)newCount);
            ctorElements[(int)existingCount] = newElement;

            newArrayInit = LLVMValueRef.CreateConstArray(
                elementType,
                ctorElements.AsSpan(0, (int)newCount)
            );
        }
        finally
        {
            if (ctorElements is not null)
                ArrayPool<LLVMValueRef>.Shared.Return(ctorElements, true);
        }

        var arrayType = LLVMTypeRef.CreateArray(elementType, newCount);
        var globalCtors = module.AddGlobal(arrayType, GlobalCtorsName);

        globalCtors.Linkage = LLVMLinkage.LLVMAppendingLinkage;
        globalCtors.Initializer = newArrayInit;
    }
}
