// @file LlvmConstantHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LLVMSharp;
using LLVMSharp.Interop;
using Type = LLVMSharp.Type;

namespace Prism.Core.Codegen;

internal static class LlvmConstantHelpers
{
    extension(ConstantInt)
    {
        public static Constant Get(Type type, ulong value, bool isSigned = false)
        {
            return ToConstInt(LLVMValueRef.CreateConstInt(type.Handle, value, isSigned));
        }

        public static Constant Get(Type type, Int128 value)
        {
            var valueSpan = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            var words = MemoryMarshal.Cast<Int128, ulong>(valueSpan);
            return ToConstInt(LLVMValueRef.CreateConstIntOfArbitraryPrecision(type.Handle, words));
        }

        public static Constant Get(Type type, UInt128 value)
        {
            var valueSpan = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            var words = MemoryMarshal.Cast<UInt128, ulong>(valueSpan);
            return ToConstInt(LLVMValueRef.CreateConstIntOfArbitraryPrecision(type.Handle, words));
        }

        public static Constant GetSigned(Type type, long value)
        {
            return ConstantInt.Get(type, unchecked((ulong)value), true);
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern ConstantInt ToConstInt(LLVMValueRef handle);

    extension(ConstantFP)
    {
        public static Constant Get(Type type, double value)
        {
            return ToConstFP(LLVMValueRef.CreateConstReal(type.Handle, value));
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern ConstantFP ToConstFP(LLVMValueRef handle);
}
