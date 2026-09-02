// @file LlvmModuleHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using LLVMSharp;
using LLVMSharp.Interop;
using Type = LLVMSharp.Type;

namespace Prism.Core.Codegen;

internal static class LlvmModuleHelpers
{
    private const string GlobalCtorsName = "llvm.global_ctors";

    public static void AppendToGlobalCtors(this Module module, Function function, int priority)
    {
        var context = module.Context;

        var int32Type = Type.GetInt32Ty(context);
        var ptrType = PointerType.Get(context);

        var elementType = StructType.Get(context, [int32Type, ptrType, ptrType], false);
        var newElement = ConstantStruct.GetAnon(
            context,
            [
                ConstantInt.Get(int32Type, unchecked((ulong)priority)),
                function,
                Constant.GetNullValue(ptrType),
            ],
            false
        );

        Constant[]? ctorElements = null;
        var existingCount = 0u;
        var newCount = 1u;

        var existingGlobal = module.GetGlobalVariable(GlobalCtorsName);

        ConstantArray newArrayInit;
        try
        {
            if (existingGlobal is not null)
            {
                var initializer = existingGlobal.Initializer;
                if (initializer is ConstantArray array)
                {
                    existingCount = array.NumOperands;
                    newCount = existingCount + 1;

                    ctorElements = ArrayPool<Constant>.Shared.Rent((int)newCount);
                    for (var i = 0u; i < existingCount; i++)
                    {
                        ctorElements[(int)i] = (Constant)array.GetOperand(i);
                    }
                }

                existingGlobal.Handle.DeleteGlobal();
            }

            ctorElements ??= ArrayPool<Constant>.Shared.Rent((int)newCount);
            ctorElements[(int)existingCount] = newElement;

            newArrayInit = ConstantArray.Get(elementType, ctorElements.AsSpan(0, (int)newCount));
        }
        finally
        {
            if (ctorElements is not null)
                ArrayPool<Constant>.Shared.Return(ctorElements, true);
        }

        var arrayType = ArrayType.Get(elementType, newCount);
        var globalCtors = module.AddGlobal(arrayType, GlobalCtorsName);

        globalCtors.Linkage = LLVMLinkage.LLVMAppendingLinkage;
        globalCtors.Initializer = newArrayInit;
    }
}
