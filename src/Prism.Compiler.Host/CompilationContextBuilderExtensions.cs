// @file CompilationContextBuilderExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Codegen.Llvm;

namespace Prism.Compiler.Host;

public static class CompilationContextBuilderExtensions
{
    extension(CompilationContext)
    {
        public static CompilationContextBuilder CreateBuilder()
        {
            return new CompilationContextBuilder().UseCodeEmitterFactory<LlvmCodeEmitterFactory>();
        }

        public static CompilationContext CreateDefault()
        {
            return CreateBuilder().Build();
        }
    }
}
