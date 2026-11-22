// @file CompilationContextBuilderExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Prism.Codegen.Llvm;
using Prism.Compiler.References;
using Prism.Metadata;

namespace Prism.Compiler.Host;

public static class CompilationContextBuilderExtensions
{
    private static readonly FileSystem DefaultFilesystem = new();

    extension(CompilationContext)
    {
        public static CompilationContextBuilder CreateBuilder()
        {
            return new CompilationContextBuilder()
                .UseCodeEmitterFactory<LlvmCodeEmitterFactory>()
                .UseAssemblyLoader<DefaultAssemblyLoader>()
                .UseReferenceResolver<FileSystemReferenceResolver>()
                .UseFileSystem(DefaultFilesystem);
        }

        public static CompilationContext CreateDefault()
        {
            return CompilationContext.CreateBuilder().Build();
        }
    }
}
