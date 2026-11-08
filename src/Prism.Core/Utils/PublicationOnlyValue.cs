// @file PublicationOnlyValue.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Utils;

internal static class PublicationOnlyValue
{
    public static PublicationOnlyValue<T> Create<T>(T value)
    {
        return new PublicationOnlyValue<T>(value);
    }
}

internal struct PublicationOnlyValue<T>
{
    private bool _initialized;
    private T _value;

    public PublicationOnlyValue(T value)
    {
        _initialized = true;
        _value = value;
    }

    public T GetOrInitialize<TState>(TState state, Func<TState, T> factory)
    {
        if (Volatile.Read(ref _initialized))
            return _value;

        var computed = factory(state);

        if (Interlocked.CompareExchange(ref _initialized, true, false))
            _value = computed;

        return _value;
    }
}
