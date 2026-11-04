// @file RegisteredLazy.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Metadata;

internal static class RegisteredLazy
{
    public static RegisteredLazy<TValue> Create<TRow, TValue>(
        RowBackingState<TRow> owner,
        Func<ReadOnlySpan<TRow>, TValue> factory
    )
        where TRow : unmanaged
        where TValue : class
    {
        return RegisteredLazy<TValue>.Create(owner, factory);
    }
}

internal readonly struct RegisteredLazy<TValue>
    where TValue : class
{
    private readonly Lazy<TValue> _lazy;

    private RegisteredLazy(Lazy<TValue> lazy)
    {
        _lazy = lazy;
    }

    public static RegisteredLazy<TValue> Create<TRow>(
        RowBackingState<TRow> owner,
        Func<ReadOnlySpan<TRow>, TValue> factory
    )
        where TRow : unmanaged
    {
        var owner1 = owner;
        return new RegisteredLazy<TValue>(
            new Lazy<TValue>(
                () => owner1.BuildLazyValue(factory),
                LazyThreadSafetyMode.ExecutionAndPublication
            )
        );
    }

    public TValue Value => _lazy.Value;
}
