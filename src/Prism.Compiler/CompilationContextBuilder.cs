// @file CompilationContextBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Prism.Compiler.Codegen;

namespace Prism.Compiler;

public sealed class CompilationContextBuilder
{
    private readonly ServiceCollection _services = [];

    internal CompilationContextBuilder UseCodeEmitter<T>()
        where T : class, ICodeEmitter
    {
        _services.AddSingleton<ICodeEmitter, T>();
        return this;
    }

    public CompilationContext Build()
    {
        return new CompilationContext(_services.BuildServiceProvider());
    }
}
