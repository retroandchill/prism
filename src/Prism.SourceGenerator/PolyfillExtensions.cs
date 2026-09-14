using System.Collections.Generic;

namespace Prism.SourceGenerator;

public static class PolyfillExtensions
{
    public static void Deconstruct<TKey, TValue>(
        this KeyValuePair<TKey, TValue> pair,
        out TKey key,
        out TValue value
    )
        where TKey : notnull
    {
        key = pair.Key;
        value = pair.Value;
    }
}
