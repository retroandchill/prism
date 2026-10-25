// @file GenerationUtils.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using ZLinq;
using ZLinq.Linq;

namespace Prism.SourceGenerator;

public static class GenerationUtils
{
    public static ValueEnumerable<FromArray<T>, T> AsValueEnumerable<T>(
        this ImmutableArray<T> source
    )
    {
        return new ValueEnumerable<FromArray<T>, T>(
            new FromArray<T>(ImmutableCollectionsMarshal.AsArray(source) ?? [])
        );
    }
}
