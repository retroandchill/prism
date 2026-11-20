// @file LazyComputation.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Prism.Utils;

internal static class LazyComputation
{
    public static T LazyCompute<T, TContext>(
        [NotNull] ref T? value,
        TContext context,
        Func<TContext, T> computation
    )
        where T : class
    {
        if (value is not null)
            return value;

        Interlocked.CompareExchange(ref value, computation(context), null);
        return value;
    }

    public static T LazyComputeUnion<T, TContext>(
        ref T value,
        TContext context,
        Func<TContext, T> computation
    )
        where T : struct, IUnion
    {
        if (value.Value is not null)
            return value;

        UnionInterlocked.Initialize(ref value, computation(context));
        return value;
    }
}
