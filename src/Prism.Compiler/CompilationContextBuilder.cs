// @file CompilationContextBuilder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Compiler.Codegen;
using Prism.Compiler.References;
using Prism.Metadata;
using Prism.Mir;

namespace Prism.Compiler;

public sealed class CompilationContextBuilder
{
    private readonly ServiceCollection _services = [];

    internal CompilationContextBuilder UseCodeEmitterFactory<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllConstructors)] T
    >()
        where T : class, ICodeEmitterFactory
    {
        _services.AddSingleton<ICodeEmitterFactory, T>();
        return this;
    }

    internal CompilationContextBuilder UseMirEmitterFactory<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllConstructors)] T
    >()
        where T : class, IMirEmitterFactory
    {
        _services.AddSingleton<IMirEmitterFactory, T>();
        return this;
    }

    public CompilationContextBuilder UseAssemblyLoader<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllConstructors)] T
    >()
        where T : class, IAssemblyLoader
    {
        _services.AddSingleton<IAssemblyLoader, T>();
        return this;
    }

    public CompilationContextBuilder UseReferenceResolver<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllConstructors)] T
    >()
        where T : class, IReferenceResolver
    {
        _services.AddSingleton<IReferenceResolver, T>();
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
