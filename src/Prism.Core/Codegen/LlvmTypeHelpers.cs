// @file LlvmTypeHelpers.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using LLVMSharp;
using LLVMSharp.Interop;
using Type = LLVMSharp.Type;

namespace Prism.Core.Codegen;

internal static class LlvmTypeHelpers
{
    extension(Type)
    {
        public static Type GetInt128Ty(LLVMContext context)
        {
            var typeRef = context.Handle.Int128Type;
            return context.GetOrCreate(typeRef);
        }
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetOrCreate")]
    private static extern Type GetOrCreate(this LLVMContext context, LLVMTypeRef typeRef);
}
