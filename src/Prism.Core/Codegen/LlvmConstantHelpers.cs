// @file LlvmConstantHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using LLVMSharp.Interop;

namespace Prism.Core.Codegen;

internal static class LlvmConstantHelpers
{
    extension(LLVMValueRef)
    {
        public static LLVMValueRef CreateConstInt(LLVMTypeRef type, Int128 value)
        {
            var valueSpan = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            var words = MemoryMarshal.Cast<Int128, ulong>(valueSpan);
            return LLVMValueRef.CreateConstIntOfArbitraryPrecision(type, words);
        }

        public static LLVMValueRef CreateConstInt(LLVMTypeRef type, UInt128 value)
        {
            var valueSpan = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            var words = MemoryMarshal.Cast<UInt128, ulong>(valueSpan);
            return LLVMValueRef.CreateConstIntOfArbitraryPrecision(type, words);
        }
    }
}
