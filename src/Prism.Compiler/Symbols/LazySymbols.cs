// @file SymbolLazy.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Prism.Compiler.Binding;

namespace Prism.Compiler.Symbols;

internal static class LazySymbols
{
    public static T LazyCompute<T, TContext>(
        [NotNull] ref T? value,
        TContext context,
        Func<TContext, BindingContext, T> computation,
        Action<TContext, BindingContext> onComplete
    )
        where T : class
    {
        if (value is not null)
            return value;

        using var bindingContext = BindingContext.Create();
        if (
            Interlocked.CompareExchange(ref value, computation(context, bindingContext), null)
            is not null
        )
            return value;

        onComplete(context, bindingContext);
        return value;
    }

    public static ImmutableArray<T> LazyCompute<T, TContext>(
        ref ImmutableArray<T> value,
        TContext context,
        Func<TContext, BindingContext, ImmutableArray<T>> computation,
        Action<TContext, BindingContext> onComplete
    )
    {
        if (!value.IsDefault)
            return value;

        using var bindingContext = BindingContext.Create();
        if (
            !ImmutableInterlocked.InterlockedInitialize(
                ref value,
                computation(context, bindingContext)
            )
        )
            return value;

        onComplete(context, bindingContext);
        return value;
    }
}
