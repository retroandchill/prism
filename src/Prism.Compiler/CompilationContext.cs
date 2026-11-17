// @file CompilationContext.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Prism.Compiler.Codegen;

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
            if (field is not null)
                return field;

            Interlocked.CompareExchange(
                ref field,
                _serviceProvider.GetRequiredService<ICodeEmitterFactory>(),
                null
            );
            return field;
        }
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}
