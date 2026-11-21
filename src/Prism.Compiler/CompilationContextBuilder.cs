// @file CompilationContextBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Compiler.Codegen;
using Prism.Metadata;

namespace Prism.Compiler;

public sealed class CompilationContextBuilder
{
    private readonly ServiceCollection _services = [];

    internal CompilationContextBuilder UseCodeEmitterFactory<T>()
        where T : class, ICodeEmitterFactory
    {
        _services.AddSingleton<ICodeEmitterFactory, T>();
        return this;
    }

    public CompilationContextBuilder UseAssemblyLoader<T>()
        where T : class, IAssemblyLoader
    {
        _services.AddSingleton<IAssemblyLoader, T>();
        return this;
    }

    public CompilationContextBuilder UseFileSystem(IFileSystem fileSystem)
    {
        _services.AddSingleton(fileSystem);
        return this;
    }

    public CompilationContext Build()
    {
        return new CompilationContext(_services.BuildServiceProvider());
    }
}
