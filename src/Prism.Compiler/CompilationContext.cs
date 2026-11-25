// @file CompilationContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Compiler.Codegen;
using Prism.Compiler.References;
using Prism.Metadata;
using Prism.Mir;
using Prism.Utils;

namespace Prism.Compiler;

public sealed class CompilationContext : IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    internal CompilationContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    internal ICodeEmitterFactory EmitterFactory
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _serviceProvider, static p =>
                p.GetRequiredService<ICodeEmitterFactory>()
            );
        }
    }

    public IFileSystem FileSystem
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _serviceProvider, static p =>
                p.GetRequiredService<IFileSystem>()
            );
        }
    }

    public IReferenceResolver ReferenceResolver
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _serviceProvider, static p =>
                p.GetRequiredService<IReferenceResolver>()
            );
        }
    }

    internal IAssemblyLoader AssemblyLoader
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _serviceProvider, static p =>
                p.GetRequiredService<IAssemblyLoader>()
            );
        }
    }

    internal IMirEmitterFactory MirEmitterFactory
    {
        get
        {
            return LazyComputation.LazyCompute(ref field, _serviceProvider, static p =>
                p.GetRequiredService<IMirEmitterFactory>()
            );
        }
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}
