// @file DictionaryExtensions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Prism.Core.Utils;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Func<TValue> factory
    )
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            return value;
        }

        value = factory();
        dict.Add(key, value);
        return value;
    }

    public static TValue GetOrAdd<TKey, TValue, TContext>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Func<TKey, TContext, TValue> factory,
        TContext context
    )
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            return value;
        }

        value = factory(key, context);
        dict.Add(key, value);
        return value;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dict,
        TKey key,
        Func<TKey, TValue> factory
    )
        where TKey : notnull
    {
        return dict.TryGetValue(key, out var value) ? value : factory(key);
    }
}
